# ESGFiap — Migração para MongoDB

Documentação da atividade **"Migração de projeto ESG para MongoDB"**. O projeto
escolhido foi o **ESGFiap**, uma API de monitoramento hídrico (ASP.NET Core 8)
que já existia neste repositório usando SQL Server + Entity Framework Core.
A aplicação inteira foi migrada: o SQL Server e o EF Core foram removidos e
substituídos por **MongoDB.Driver**, incluindo a camada de autenticação
(ASP.NET Identity foi trocado por um `PasswordHasher` + coleção `users`
própria).

> Este documento cobre as 4 seções pedidas no enunciado: (1) decisão e
> justificativa, (2) as 5 collections e seu papel ESG, (3) comandos de
> criação e operações CRUD, (4) evidências de execução.

---

## 1. Decisão: Migração (não inovação)

Optei por **migrar** o projeto existente em vez de criar um novo, porque o
domínio (sensores de água, leituras de consumo, alertas de vazamento e
relatórios de impacto hídrico) já é naturalmente **orientado a documentos** e
se beneficia diretamente do modelo NoSQL:

- **Dados de sensores são heterogêneos por natureza.** Um sensor de vazão
  (`Flow`), um de pressão (`Pressure`) e um detector de vazamento
  (`LeakDetector`) têm campos técnicos completamente diferentes
  (diâmetro de tubulação vs. pressão máxima vs. sensibilidade acústica). Em
  SQL isso exigiria tabelas separadas ou uma tabela cheia de colunas `NULL`;
  em MongoDB é um único subdocumento `Specs` cujo formato varia por
  documento — sem migração de schema.
- **Telemetria de alto volume (`readings`) não precisa de um schema rígido.**
  Nem toda leitura carrega metadados de qualidade/anomalia — só quando o
  pipeline de detecção de vazamento sinaliza algo suspeito. Um campo opcional
  (`Quality`) resolve isso sem coluna `NULL` em 99% das linhas.
- **Relatórios de ESG são inerentemente polimórficos.** Um relatório de
  impacto hídrico, uma auditoria de conformidade e um relatório de
  engajamento comunitário compartilham campos comuns (período, título,
  região), mas cada um carrega métricas ambientais, sociais ou de governança
  bem diferentes. O campo `EsgMetrics` (documento livre) modela isso
  diretamente, meta que o relacional só alcançaria com várias tabelas ou com
  colunas JSON "escondidas" — perdendo a vantagem do NoSQL nativo.
- **Consultas típicas do domínio são "por dono" e "por período/sensor"**,
  exatamente o padrão que índices compostos do Mongo (`UserId + CreatedAt`,
  `SensorId + ReadingAt`) atendem bem, sem precisar de `JOIN`s.

Em resumo: o NoSQL não é usado aqui só "porque pode" — ele resolve um
problema real do domínio ESG, que é **captar dados ambientais, sociais e de
governança que não têm um formato único** entre organizações, tipos de
usuário e tipos de sensor.

---

## 2. Arquitetura: antes → depois

| Antes (SQL Server + EF Core)                          | Depois (MongoDB)                                              |
|---------------------------------------------------------|-----------------------------------------------------------------|
| `AppDbContext : IdentityDbContext<ApplicationUser>`      | `MongoContext` com 5 `IMongoCollection<T>`                     |
| ASP.NET Identity (`UserManager`, `RoleManager`, EF store) | `PasswordHasher<UserDocument>` + CRUD manual na collection `users` |
| Chaves primárias `int` autoincremento                    | `_id` do tipo `ObjectId` (representado como `string` no C#)     |
| Tabelas `WaterSensors`, `WaterReadings`, `LeakAlerts`, `WaterImpactReports`, `AspNetUsers` (5 tabelas relacionais + tabelas de Identity) | Collections `users`, `sensors`, `readings`, `alerts`, `impact_reports` |
| Relacionamentos via FK + `Include()`                      | Referência por id (`UserId`, `SensorId`) + campos denormalizados (`SensorName`, `UserName`) para evitar `$lookup` nas consultas mais comuns |
| Migrations do EF Core (`Data/Migrations`)                 | Sem migrations — schema flexível por documento; índices criados em `MongoContext.EnsureIndexesAsync()` no startup |

Pacotes removidos: `Microsoft.EntityFrameworkCore.SqlServer`,
`Microsoft.EntityFrameworkCore.Design`,
`Microsoft.AspNetCore.Identity.EntityFrameworkCore`.
Pacotes adicionados: `MongoDB.Driver`, `Microsoft.Extensions.Identity.Core`
(só para reaproveitar o utilitário `PasswordHasher<T>`, sem o restante do
Identity).

`docker-compose.yml` trocou o serviço `sqlserver` (`mssql/server`) por
`mongo` (`mongo:8.0`).

---

## 3. As 5 collections e seu papel ESG

### 3.1 `users` — Governança
Quem é o dono dos sensores/relatórios e qual o seu compromisso ESG.
Residencial vs. Comercial, papéis (`Roles`) para controle de acesso
(`[Authorize(Roles = "Admin")]` no `AlertController`), e um perfil de
sustentabilidade cujo formato **muda por tipo de usuário** — empresas
carregam certificações e metas, e nem todo usuário possui esse campo.

```json
{
  "_id": ObjectId("..."),
  "Email": "maria@aquatech.com",
  "FullName": "Maria Silva",
  "CompanyName": "AquaTech Ltda",
  "UserType": "Commercial",
  "Roles": ["User"],
  "SustainabilityProfile": {
    "certifications": ["ISO14001"],
    "waterReuseProgram": true,
    "annualReductionGoalPercent": 15
  }
}
```
```json
{
  "_id": ObjectId("..."),
  "Email": "carlos.souza@example.com",
  "FullName": "Carlos Souza",
  "UserType": "Residential",
  "Roles": ["User"]
  // sem SustainabilityProfile — campo opcional, ausente neste documento
}
```

### 3.2 `sensors` — Ambiental
Pontos de monitoramento de água. Campo `Specs` é um subdocumento **cujo
formato depende de `Type`**:

```json
{ "Type": "Flow",         "Specs": { "pipeDiameterMm": 50, "maxFlowLitersPerMinute": 120 } }
{ "Type": "Pressure",     "Specs": { "calibrationDate": "2026-01-15", "maxPressureBar": 8.5 } }
{ "Type": "LeakDetector", "Specs": { "sensitivityLevel": "high", "acousticRangeMeters": 30 } }
```

### 3.3 `readings` — Ambiental
Telemetria bruta de consumo/pressão. O campo `Quality` só existe em leituras
marcadas como anômalas pelo `LeakDetectionService` — a maioria dos
documentos não o possui.

```json
{ "LitersPerHour": 45, "PressureBar": 2.5 }
{ "LitersPerHour": 250, "PressureBar": 3.4, "Quality": { "status": "anomaly", "confidence": 0.82, "notes": "..." } }
```

### 3.4 `alerts` — Social + Ambiental
Alertas de vazamento/consumo excessivo, gerados automaticamente pelo
`LeakDetectionService` a cada leitura acima do limite do sensor — o impacto
social é a resposta operacional rápida que evita desperdício e danos à
comunidade. `ResolutionActions` é um array que **não existe até a primeira
resolução** — quando o operador resolve o alerta, o backend faz um `$push`
que cria o campo dinamicamente:

```json
{ "Status": "Open", "Severity": "Critical" /* sem ResolutionActions */ }
{
  "Status": "Resolved",
  "ResolutionActions": [
    { "action": "Resolved", "by": ObjectId("..."), "at": ISODate("...") }
  ]
}
```

### 3.5 `impact_reports` — Ambiental + Social + Governança (combinados)
A collection que mais demonstra a flexibilidade do modelo: `ReportType`
decide quais seções existem dentro de `EsgMetrics`.

```json
{ "ReportType": "WaterImpact",         "EsgMetrics": { "environmental": { "waterSavedLiters": 12000, "leaksResolved": 2 } } }
{ "ReportType": "ComplianceAudit",     "EsgMetrics": { "governance": { "auditedBy": "ExternalCo Auditores", "complianceScore": 92 }, "social": { "communitiesNotified": 1 } } }
{ "ReportType": "CommunityEngagement", "EsgMetrics": { "social": { "communitiesNotified": 6, "workshopsHeld": 3, "beneficiaries": 240 } } }
```

---

## 4. Comandos de criação e operações CRUD

Duas fontes de evidência, ambas reais (executadas neste ambiente, não
simuladas):

1. **Via API real** (`mongodb/seed_via_api.py`) — chama o próprio backend
   ASP.NET Core migrado, que por baixo usa `MongoDB.Driver` para inserir,
   consultar, atualizar e apagar documentos. Prova que a migração da
   aplicação funciona ponta a ponta.
2. **Via `mongosh` direto** (`mongodb/crud_demo.js`) — script autocontido que
   cria as 5 collections do zero (`db.createCollection`) e executa
   `insertOne/insertMany`, `find`, `updateOne/updateMany` (incluindo `$push`
   em campo inexistente) e `deleteOne/deleteMany`, sem depender da aplicação
   rodando. Ideal para reproduzir e tirar prints isoladamente.

### 4.1 Subindo o MongoDB local

```bash
docker compose up -d mongo
# ou, sem compose:
docker run -d --name esgfiap-mongo -p 27017:27017 \
  -e MONGO_INITDB_ROOT_USERNAME=esgfiap -e MONGO_INITDB_ROOT_PASSWORD='ESGFiap@2024!' \
  mongo:8.0
```

### 4.2 Criação das collections

As collections são criadas de duas formas equivalentes:

- **Implícita**: o driver do MongoDB cria a collection automaticamente no
  primeiro `InsertOneAsync` de cada `IMongoCollection<T>` exposto por
  `Data/MongoContext.cs` (`users`, `sensors`, `readings`, `alerts`,
  `impact_reports`). Isso acontece assim que a API roda e processa o
  primeiro registro de usuário/sensor/leitura/relatório.
- **Explícita**: `mongodb/crud_demo.js`, seção 1, roda
  `db.createCollection("users")`, `db.createCollection("sensors")`,
  `db.createCollection("readings")`, `db.createCollection("alerts")`,
  `db.createCollection("impact_reports")` diretamente, para deixar o comando
  de criação registrado e visível.

Índices (equivalente a "constraints" do modelo relacional) são criados no
startup da aplicação, em `MongoContext.EnsureIndexesAsync()`:

```csharp
await Users.Indexes.CreateOneAsync(new CreateIndexModel<UserDocument>(
    Builders<UserDocument>.IndexKeys.Ascending(u => u.NormalizedEmail),
    new CreateIndexOptions { Unique = true }));
// + índices em sensors.UserId, readings.SensorId+ReadingAt,
//   alerts.UserId+DetectedAt, impact_reports.UserId+CreatedAt
```

Evidência real (`mongosh`, banco usado pela API rodando):

```
--- indexes: users ---
[ { key: { _id: 1 }, name: '_id_' },
  { key: { NormalizedEmail: 1 }, name: 'ux_users_normalizedEmail', unique: true } ]
--- indexes: readings ---
[ { key: { _id: 1 }, name: '_id_' },
  { key: { SensorId: 1, ReadingAt: -1 }, name: 'ix_readings_sensorId_readingAt' } ]
```

### 4.3 Seed com 10+ documentos por collection (via API real)

```bash
dotnet run --project ESGFiap.csproj &          # sobe a API em localhost:5257
python3 mongodb/seed_via_api.py                # popula os dados abaixo
```

Resultado real de uma execução (transcrito, sem edição):

```
== 1) users (register) ==
  registered maria@aquatech.com (200)
  ... (10 usuários)

== 2) sensors (create, flexible Specs per type) ==
  Sensor Entrada -> 6a84b5dbb036f0bc4812d229 (201)
  ... (13 sensores)

== 3) readings (some over threshold -> auto alerts; some with Quality flag) ==
  readings created, alerts auto-generated: 11

== 4) resolve/acknowledge a few alerts (update -> adds ResolutionActions) ==
  maria@aquatech.com: updated 4 alert(s)
  ana@ecofarms.com: updated 2 alert(s)
  rafael@metalworks.com: updated 2 alert(s)

== 5) impact reports (flexible EsgMetrics per ReportType) ==
  Relatorio Hidrico Q1 2026 -> 6a84b5dcb036f0bc4812d2a2 (201) total=576
  ... (10 relatórios)

Done.
```

Contagem final por collection (banco `esgfiap`, usado pela API):

| Collection        | Documentos |
|-------------------|-----------:|
| `users`            | 10          |
| `sensors`          | 13          |
| `readings`         | 31          |
| `alerts`           | 11          |
| `impact_reports`   | 10          |

### 4.4 CRUD via a própria API (evidência real de chamadas HTTP → Mongo)

```bash
# CREATE (insert) — cria usuário (collection users)
curl -X POST http://localhost:5257/api/auth/register -H 'Content-Type: application/json' -d '{
  "fullName":"Maria Silva","email":"maria@example.com","password":"Senha123!",
  "companyName":"AquaTech Ltda","userType":2
}'

# CREATE (insert) — cria sensor com Specs flexível (collection sensors)
curl -X POST http://localhost:5257/api/sensor -H "Authorization: Bearer $TOKEN" -d '{
  "name":"Sensor Entrada","location":"Portaria principal","type":1,
  "alertThresholdLitersPerHour":50,
  "specs":{"pipeDiameterMm":50,"maxFlowLitersPerMinute":120}
}'

# CREATE (insert) — leitura acima do limite gera alerta automaticamente (readings + alerts)
curl -X POST http://localhost:5257/api/sensor/$SENSOR_ID/readings -H "Authorization: Bearer $TOKEN" -d '{
  "litersPerHour":250,"pressureBar":3.2
}'
# resposta: {"reading":{...},"alertGenerated":true}

# READ (find) — lista alertas do usuário autenticado
curl http://localhost:5257/api/alert -H "Authorization: Bearer $TOKEN"

# UPDATE — resolve alerta (adiciona ResolutionActions via $push, campo antes inexistente)
curl -X PATCH http://localhost:5257/api/alert/$ALERT_ID/status -H "Authorization: Bearer $TOKEN" -d '{"status":3}'

# DELETE — remove sensor (e cascata lógica tratada pela aplicação)
curl -X DELETE http://localhost:5257/api/sensor/$SENSOR_ID -H "Authorization: Bearer $TOKEN"
```

Saída real observada (transcrita):

```json
{"reading":{"id":"6a84b53ff15514ea649062cd","litersPerHour":250,"pressureBar":3.2,
 "readingAt":"2026-08-18T19:40:47.75014Z","sensorId":"6a84b53ff15514ea649062cb",
 "sensorName":"Sensor Entrada","quality":null},"alertGenerated":true}
```
```json
{"id":"6a84b53ff15514ea649062ce","message":"Consumo de 250,0 L/h detectado em 'Portaria principal'. Limite configurado: 50,0 L/h (400% acima do limite).",
 "severity":"Critical","status":"Resolved","resolvedAt":"2026-08-18T19:40:57.132Z",
 "resolutionActions":[{"action":"Resolved","by":"6a84b53ff15514ea649062ca","at":{"$date":"2026-08-18T19:40:57.132Z"}}]}
```

Documento correspondente direto no Mongo (`mongosh`, mostrando o `$push`
funcionando de fato no banco, não só na resposta HTTP):

```js
db.alerts.findOne({ _id: ObjectId('6a84b53ff15514ea649062ce') })
```
```json
{
  "_id": "ObjectId('6a84b53ff15514ea649062ce')",
  "Status": "Resolved",
  "ResolvedAt": "ISODate('2026-08-18T19:40:57.132Z')",
  "ResolutionActions": [
    { "action": "Resolved", "by": "6a84b53ff15514ea649062ca", "at": "ISODate('2026-08-18T19:40:57.132Z')" }
  ]
}
```

### 4.5 CRUD direto via `mongosh` (independente da API)

```bash
docker exec -i esgfiap-mongo mongosh -u esgfiap -p 'ESGFiap@2024!' \
  --authenticationDatabase admin --quiet < mongodb/crud_demo.js
```

O script (banco `esgfiap_demo`, isolado do banco usado pela API) executa, na
ordem: `drop` → `createCollection` (5×) → `insertOne`/`insertMany` (10+ por
collection) → `find` com filtros e agregação → `updateOne`/`updateMany`
(incluindo `$push` em campo ausente e adição de campo dinâmico em `Specs`) →
`deleteOne`/`deleteMany`. Trechos reais da execução:

```
================ 1) CREATE COLLECTIONS ================
[ 'users', 'sensors', 'readings', 'alerts', 'impact_reports' ]

-- updateOne: resolve an alert and $push a ResolutionActions entry (field did not exist before) --
{ acknowledged: true, insertedId: null, matchedCount: 1, modifiedCount: 1, upsertedCount: 0 }
resolved alert now has ResolutionActions:
{
  _id: ObjectId('6a84b63a491a6bad13b2b5c2'),
  SensorName: 'Sensor Pressao Bomba',
  Status: 'Resolved',
  ResolutionActions: [ { action: 'Resolved', by: ObjectId('...'), at: ISODate('...') } ],
  ResolvedAt: ISODate('2026-08-18T19:44:58.942Z')
}

-- updateMany: deactivate every LeakDetector sensor for maintenance --
{ acknowledged: true, matchedCount: 3, modifiedCount: 3 }

-- deleteMany: purge Low-severity alerts older than the demo window --
{ acknowledged: true, deletedCount: 2 }

================ FINAL COUNTS ================
users: 10
sensors: 13
readings: 30
alerts: 9
impact_reports: 10
```

(A contagem final de `alerts` cai para 9 porque a própria seção 9 do script
apaga 2 alertas de severidade `Low` como demonstração de `deleteMany` — as
outras 4 collections seguem com 10+ documentos.)

---

## 5. Prints de tela

Este ambiente não tem interface gráfica, então as "capturas" acima são
**transcrições reais e não editadas** da execução dos comandos (terminal +
`mongosh` + respostas HTTP da API). Para o print de tela propriamente dito
exigido na entrega, rode localmente:

```bash
docker compose up -d mongo
docker exec -i esgfiap-mongo mongosh -u esgfiap -p 'ESGFiap@2024!' \
  --authenticationDatabase admin --quiet < mongodb/crud_demo.js
```

e capture a tela do terminal (ou abra o MongoDB Compass em
`mongodb://esgfiap:ESGFiap%402024!@localhost:27017` para navegar
visualmente pelas 5 collections e suas operações).

---

## 6. Como reproduzir

```bash
# 1. Suba o MongoDB
docker compose up -d mongo

# 2. Rode a API (usa appsettings.Development.json -> localhost:27017)
dotnet run --project ESGFiap.csproj

# 3a. Popule via API real (Python 3, sem dependências externas)
python3 mongodb/seed_via_api.py

# 3b. OU popule/demonstre CRUD direto no banco, sem precisar da API
docker exec -i esgfiap-mongo mongosh -u esgfiap -p 'ESGFiap@2024!' \
  --authenticationDatabase admin --quiet < mongodb/crud_demo.js

# 4. Rode os testes de integração (usam um banco Mongo real e efêmero)
dotnet test tests/ESGFiap.Tests/ESGFiap.Tests.csproj
```

---

## 7. Mapeamento com os critérios de avaliação

| Critério                                                      | Onde foi endereçado |
|-----------------------------------------------------------------|------------------------|
| Aderência ao tema ESG                                            | Domínio inteiro é monitoramento hídrico; cada collection foi desenhada em torno de um pilar E/S/G (seção 3) |
| Adequação da estrutura NoSQL ao problema                         | Seção 1 (justificativa) + `Specs`/`Quality`/`EsgMetrics` resolvendo variação real de schema |
| Completude e qualidade das operações CRUD                        | Seção 4: CRUD completo via API real (`Controllers/*.cs`, usando `MongoDB.Driver`) e via `mongosh` (`crud_demo.js`) |
| Clareza e profundidade da documentação técnica                   | Este documento + comentários nos modelos (`Models/*.cs`) explicando cada campo flexível |
| Representação coerente de dados flexíveis com o MongoDB           | `BsonDocument`/`JsonElement` nos modelos e ViewModels (`Models/BsonJson.cs`), com exemplos concretos de documentos com formatos diferentes na mesma collection |
