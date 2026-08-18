// ESGFiap – MongoDB collection creation + CRUD demonstration script.
//
// Self-contained: does NOT require the .NET API to be running. Targets a
// dedicated database ("esgfiap_demo") so it can be re-run from a clean slate
// without touching data produced by the live application (database "esgfiap").
//
// Run with:
//   mongosh "mongodb://esgfiap:ESGFiap%402024!@localhost:27017" mongodb/crud_demo.js
// or, against the docker-compose container:
//   docker exec -i esgfiap-mongo mongosh -u esgfiap -p 'ESGFiap@2024!' --authenticationDatabase admin < mongodb/crud_demo.js

db = db.getSiblingDB("esgfiap_demo");

print("\n================ 0) DROP (clean slate) ================");
db.users.drop();
db.sensors.drop();
db.readings.drop();
db.alerts.drop();
db.impact_reports.drop();

print("\n================ 1) CREATE COLLECTIONS ================");
db.createCollection("users");
db.createCollection("sensors");
db.createCollection("readings");
db.createCollection("alerts");
db.createCollection("impact_reports");
print(db.getCollectionNames());

// ---------------------------------------------------------------------
// 2) USERS — governance: who owns sensors/reports, ESG commitments per org.
// Flexible schema: SustainabilityProfile shape differs by UserType, and is
// simply absent on some documents.
// ---------------------------------------------------------------------
print("\n================ 2) INSERT: users ================");
var userIds = {};
var userDocs = [
  { key: "maria",    Email: "maria@aquatech.com",        NormalizedEmail: "MARIA@AQUATECH.COM",        FullName: "Maria Silva",     CompanyName: "AquaTech Ltda",       UserType: "Commercial",  Roles: ["User"], SustainabilityProfile: { certifications: ["ISO14001"], waterReuseProgram: true, annualReductionGoalPercent: 15 } },
  { key: "joao",     Email: "joao.pereira@example.com",  NormalizedEmail: "JOAO.PEREIRA@EXAMPLE.COM",  FullName: "Joao Pereira",    CompanyName: "",                    UserType: "Residential", Roles: ["User"], SustainabilityProfile: { hasRainwaterHarvesting: true, dailyGoalLiters: 150 } },
  { key: "ana",      Email: "ana@ecofarms.com",          NormalizedEmail: "ANA@ECOFARMS.COM",          FullName: "Ana Costa",       CompanyName: "EcoFarms",            UserType: "Commercial",  Roles: ["User"], SustainabilityProfile: { certifications: ["B-Corp"], irrigationEfficiency: "drip" } },
  { key: "carlos",   Email: "carlos.souza@example.com",  NormalizedEmail: "CARLOS.SOUZA@EXAMPLE.COM",  FullName: "Carlos Souza",    CompanyName: "",                    UserType: "Residential", Roles: ["User"] },
  { key: "beatriz",  Email: "beatriz@techpark.com",      NormalizedEmail: "BEATRIZ@TECHPARK.COM",      FullName: "Beatriz Lima",    CompanyName: "TechPark Offices",    UserType: "Commercial",  Roles: ["User"], SustainabilityProfile: { certifications: ["LEED Gold"], greyWaterRecycling: true } },
  { key: "fernanda", Email: "fernanda.alves@example.com",NormalizedEmail: "FERNANDA.ALVES@EXAMPLE.COM",FullName: "Fernanda Alves",  CompanyName: "",                    UserType: "Residential", Roles: ["User"], SustainabilityProfile: { smartIrrigation: true } },
  { key: "rafael",   Email: "rafael@metalworks.com",     NormalizedEmail: "RAFAEL@METALWORKS.COM",     FullName: "Rafael Nunes",    CompanyName: "MetalWorks Industria",UserType: "Commercial",  Roles: ["User"], SustainabilityProfile: { certifications: ["ISO50001", "ISO14001"], effluentTreatmentPlant: true } },
  { key: "juliana",  Email: "juliana.rocha@example.com", NormalizedEmail: "JULIANA.ROCHA@EXAMPLE.COM", FullName: "Juliana Rocha",   CompanyName: "",                    UserType: "Residential", Roles: ["User"] },
  { key: "pedro",    Email: "pedro@hotelcostaazul.com",  NormalizedEmail: "PEDRO@HOTELCOSTAAZUL.COM",  FullName: "Pedro Martins",   CompanyName: "Hotel Costa Azul",    UserType: "Commercial",  Roles: ["User", "Admin"], SustainabilityProfile: { certifications: ["Green Key"], poolWaterRecyclingPercent: 80 } },
  { key: "camila",   Email: "camila.ferreira@example.com",NormalizedEmail:"CAMILA.FERREIRA@EXAMPLE.COM",FullName: "Camila Ferreira",CompanyName: "",                    UserType: "Residential", Roles: ["User"], SustainabilityProfile: { leakAlertSubscribed: true } }
];
userDocs.forEach(function (u) {
  var doc = {
    Email: u.Email, NormalizedEmail: u.NormalizedEmail, PasswordHash: "AQAAAAIAAYagAAAAE...(hashed)",
    FullName: u.FullName, CompanyName: u.CompanyName, UserType: u.UserType, Roles: u.Roles,
    CreatedAt: new Date()
  };
  if (u.SustainabilityProfile) doc.SustainabilityProfile = u.SustainabilityProfile;
  var res = db.users.insertOne(doc);
  userIds[u.key] = res.insertedId;
});
print("users inserted:", db.users.countDocuments());

// ---------------------------------------------------------------------
// 3) SENSORS — environmental: monitoring points. Flexible schema: Specs
// fields differ by sensor Type (Flow / Pressure / LeakDetector).
// ---------------------------------------------------------------------
print("\n================ 3) INSERT: sensors ================");
var sensorIds = {};
var sensorDocs = [
  { key: "entrada",     UserId: userIds.maria,   Name: "Sensor Entrada",               Location: "Portaria principal",      Type: "Flow",         AlertThresholdLitersPerHour: 50,  Specs: { pipeDiameterMm: 50, maxFlowLitersPerMinute: 120 } },
  { key: "pressao",     UserId: userIds.maria,   Name: "Sensor Pressao Bomba",          Location: "Casa de bombas",           Type: "Pressure",     AlertThresholdLitersPerHour: 40,  Specs: { calibrationDate: "2026-01-15", maxPressureBar: 8.5 } },
  { key: "vazamento1",  UserId: userIds.maria,   Name: "Detector Vazamento Subsolo",    Location: "Garagem -1",               Type: "LeakDetector", AlertThresholdLitersPerHour: 5,   Specs: { sensitivityLevel: "high", acousticRangeMeters: 30 } },
  { key: "irrigA",      UserId: userIds.ana,     Name: "Irrigacao Setor A",             Location: "Talhao A",                 Type: "Flow",         AlertThresholdLitersPerHour: 300, Specs: { pipeDiameterMm: 100, maxFlowLitersPerMinute: 500, soilMoistureThresholdPercent: 35 } },
  { key: "irrigB",      UserId: userIds.ana,     Name: "Irrigacao Setor B",             Location: "Talhao B",                 Type: "Flow",         AlertThresholdLitersPerHour: 250, Specs: { pipeDiameterMm: 80, maxFlowLitersPerMinute: 400, soilMoistureThresholdPercent: 40 } },
  { key: "vazamento2",  UserId: userIds.ana,     Name: "Detector Vazamento Reservatorio", Location: "Reservatorio central",   Type: "LeakDetector", AlertThresholdLitersPerHour: 5,   Specs: { sensitivityLevel: "medium", acousticRangeMeters: 50 } },
  { key: "torre1",      UserId: userIds.beatriz, Name: "Sensor Torre 1",                Location: "Torre 1 - caixa dagua",    Type: "Pressure",     AlertThresholdLitersPerHour: 60,  Specs: { calibrationDate: "2025-11-02", maxPressureBar: 10 } },
  { key: "torre2",      UserId: userIds.beatriz, Name: "Sensor Torre 2",                Location: "Torre 2 - entrada",         Type: "Flow",         AlertThresholdLitersPerHour: 80,  Specs: { pipeDiameterMm: 60, maxFlowLitersPerMinute: 150 } },
  { key: "industrial",  UserId: userIds.rafael,  Name: "Sensor Processo Industrial",    Location: "Linha de producao 3",      Type: "Flow",         AlertThresholdLitersPerHour: 500, Specs: { pipeDiameterMm: 150, maxFlowLitersPerMinute: 900, industrialGrade: true } },
  { key: "tanque",      UserId: userIds.rafael,  Name: "Detector Vazamento Tanque",     Location: "Patio de tanques",         Type: "LeakDetector", AlertThresholdLitersPerHour: 5,   Specs: { sensitivityLevel: "high", acousticRangeMeters: 40 } },
  { key: "piscina",     UserId: userIds.pedro,   Name: "Sensor Piscina",                Location: "Area da piscina",           Type: "Pressure",     AlertThresholdLitersPerHour: 35,  Specs: { calibrationDate: "2026-02-20", maxPressureBar: 6 } },
  { key: "cozinha",     UserId: userIds.pedro,   Name: "Sensor Cozinha",                Location: "Cozinha industrial",        Type: "Flow",         AlertThresholdLitersPerHour: 90,  Specs: { pipeDiameterMm: 40, maxFlowLitersPerMinute: 100 } },
  { key: "caixaJoao",   UserId: userIds.joao,    Name: "Sensor Caixa Dagua",            Location: "Telhado",                   Type: "Flow",         AlertThresholdLitersPerHour: 30,  Specs: { pipeDiameterMm: 25, maxFlowLitersPerMinute: 50 } }
];
sensorDocs.forEach(function (s) {
  var res = db.sensors.insertOne({
    UserId: s.UserId, Name: s.Name, Location: s.Location, Type: s.Type, IsActive: true,
    AlertThresholdLitersPerHour: s.AlertThresholdLitersPerHour, InstalledAt: new Date(), Specs: s.Specs
  });
  sensorIds[s.key] = res.insertedId;
});
print("sensors inserted:", db.sensors.countDocuments());

// ---------------------------------------------------------------------
// 4) READINGS — environmental: raw telemetry. Flexible schema: Quality is
// only present on readings flagged as anomalous by the detection pipeline.
// ---------------------------------------------------------------------
print("\n================ 4) INSERT: readings ================");
function hoursAgo(h) { return new Date(Date.now() - h * 3600 * 1000); }
var readingPlan = [
  ["entrada",    "Sensor Entrada",   [45, 48, 60, 250]],
  ["pressao",    "Sensor Pressao Bomba", [30, 35, 90]],
  ["vazamento1", "Detector Vazamento Subsolo", [1, 2, 15]],
  ["irrigA",     "Irrigacao Setor A", [280, 295, 400]],
  ["irrigB",     "Irrigacao Setor B", [230, 245, 260]],
  ["torre1",     "Sensor Torre 1", [55, 58, 120]],
  ["industrial", "Sensor Processo Industrial", [480, 495, 700]],
  ["tanque",     "Detector Vazamento Tanque", [1, 3, 12]],
  ["piscina",    "Sensor Piscina", [30, 33, 55]],
  ["caixaJoao",  "Sensor Caixa Dagua", [25, 28, 45]]
];
var readingDocs = [];
readingPlan.forEach(function (p) {
  var sensorKey = p[0], sensorName = p[1], values = p[2];
  values.forEach(function (v, i) {
    var doc = {
      SensorId: sensorIds[sensorKey], SensorName: sensorName, LitersPerHour: v,
      PressureBar: Math.round((2.5 + i * 0.3) * 10) / 10, ReadingAt: hoursAgo(values.length - i)
    };
    if (i === values.length - 1) {
      doc.Quality = { status: "anomaly", confidence: 0.82, notes: "Consumo muito acima do padrao historico" };
    }
    readingDocs.push(doc);
  });
});
db.readings.insertMany(readingDocs);
print("readings inserted:", db.readings.countDocuments());

// ---------------------------------------------------------------------
// 5) ALERTS — social + environmental: leak/consumption alerts raised for
// operators. Flexible schema: ResolutionActions only exists once an alert
// has been worked (added later via $push, see UPDATE section below).
// ---------------------------------------------------------------------
print("\n================ 5) INSERT: alerts ================");
var alertPlan = [
  ["entrada", "Sensor Entrada", "Portaria principal", "maria", 250, "Critical"],
  ["pressao", "Sensor Pressao Bomba", "Casa de bombas", "maria", 90, "High"],
  ["vazamento1", "Detector Vazamento Subsolo", "Garagem -1", "maria", 15, "Critical"],
  ["irrigA", "Irrigacao Setor A", "Talhao A", "ana", 400, "Medium"],
  ["irrigB", "Irrigacao Setor B", "Talhao B", "ana", 260, "Low"],
  ["torre1", "Sensor Torre 1", "Torre 1 - caixa dagua", "beatriz", 120, "High"],
  ["industrial", "Sensor Processo Industrial", "Linha de producao 3", "rafael", 700, "Medium"],
  ["tanque", "Detector Vazamento Tanque", "Patio de tanques", "rafael", 12, "Critical"],
  ["piscina", "Sensor Piscina", "Area da piscina", "pedro", 55, "Medium"],
  ["caixaJoao", "Sensor Caixa Dagua", "Telhado", "joao", 45, "Low"],
  ["torre2", "Sensor Torre 2", "Torre 2 - entrada", "beatriz", 200, "High"]
];
var alertIds = [];
alertPlan.forEach(function (a) {
  var res = db.alerts.insertOne({
    SensorId: sensorIds[a[0]], SensorName: a[1], SensorLocation: a[2], UserId: userIds[a[3]],
    Message: "Consumo de " + a[4] + " L/h detectado em '" + a[2] + "'.",
    Severity: a[5], Status: "Open", DetectedLitersPerHour: a[4], DetectedAt: new Date()
  });
  alertIds.push(res.insertedId);
});
print("alerts inserted:", db.alerts.countDocuments());

// ---------------------------------------------------------------------
// 6) IMPACT_REPORTS — environmental + social + governance combined.
// Flexible/polymorphic schema: EsgMetrics sections depend on ReportType.
// ---------------------------------------------------------------------
print("\n================ 6) INSERT: impact_reports ================");
var reportPlan = [
  ["maria", "Relatorio Hidrico Q1 2026", "Sao Paulo - SP", "WaterImpact", { environmental: { waterSavedLiters: 12000, leaksResolved: 2 } }],
  ["maria", "Auditoria de Conformidade 2026", "Sao Paulo - SP", "ComplianceAudit", { governance: { auditedBy: "ExternalCo Auditores", complianceScore: 92 }, social: { communitiesNotified: 1 } }],
  ["ana", "Impacto Hidrico da Irrigacao Q1", "Ribeirao Preto - SP", "WaterImpact", { environmental: { waterSavedLiters: 45000, irrigationEfficiencyGainPercent: 18 } }],
  ["ana", "Engajamento Comunitario - Bacia do Rio Pardo", "Ribeirao Preto - SP", "CommunityEngagement", { social: { communitiesNotified: 6, workshopsHeld: 3, beneficiaries: 240 } }],
  ["beatriz", "Relatorio Hidrico Torres Q1", "Campinas - SP", "WaterImpact", { environmental: { waterSavedLiters: 8000 } }],
  ["beatriz", "Auditoria LEED Gold 2026", "Campinas - SP", "ComplianceAudit", { governance: { auditedBy: "GBC Brasil", certificationStatus: "Renewed" } }],
  ["rafael", "Relatorio de Efluentes Industriais", "Sorocaba - SP", "WaterImpact", { environmental: { waterSavedLiters: 30000, effluentTreatedPercent: 95 } }],
  ["rafael", "Auditoria ISO 50001", "Sorocaba - SP", "ComplianceAudit", { governance: { auditedBy: "Bureau Veritas", complianceScore: 88 } }],
  ["pedro", "Relatorio Hidrico Hotel Q1", "Guaruja - SP", "WaterImpact", { environmental: { waterSavedLiters: 15000, poolRecyclingPercent: 80 } }],
  ["pedro", "Engajamento com Hospedes - Consumo Consciente", "Guaruja - SP", "CommunityEngagement", { social: { guestsEducated: 500, towelReuseAdoptionPercent: 63 } }]
];
reportPlan.forEach(function (r) {
  db.impact_reports.insertOne({
    UserId: userIds[r[0]], UserName: r[0], Title: r[1], Region: r[2],
    PeriodStart: hoursAgo(24 * 90), PeriodEnd: new Date(),
    TotalConsumedLiters: 0, RegionalAverageLiters: 20000,
    ImpactAnalysis: "Analise de impacto para " + r[1] + ".",
    MitigationPlan: "Manter monitoramento continuo e planos de reuso.",
    CreatedAt: new Date(), ReportType: r[3], EsgMetrics: r[4]
  });
});
print("impact_reports inserted:", db.impact_reports.countDocuments());

// =========================== READ (find) ===========================
print("\n================ 7) READ: find examples ================");
print("-- one flexible sensor doc (Flow) --");
printjson(db.sensors.findOne({ Type: "Flow" }));
print("-- one flexible sensor doc (LeakDetector), different Specs shape --");
printjson(db.sensors.findOne({ Type: "LeakDetector" }));
print("-- open, high-severity alerts --");
printjson(db.alerts.find({ Status: "Open", Severity: { $in: ["High", "Critical"] } }).toArray());
print("-- commercial users with a SustainabilityProfile --");
printjson(db.users.find({ UserType: "Commercial", SustainabilityProfile: { $exists: true } }, { Email: 1, CompanyName: 1, SustainabilityProfile: 1 }).toArray());
print("-- readings flagged as anomalies --");
printjson(db.readings.find({ "Quality.status": "anomaly" }).toArray());
print("-- reports by type, aggregated --");
printjson(db.impact_reports.aggregate([{ $group: { _id: "$ReportType", count: { $sum: 1 } } }]).toArray());

// =========================== UPDATE ===========================
print("\n================ 8) UPDATE examples ================");
print("-- updateOne: acknowledge one alert --");
printjson(db.alerts.updateOne({ _id: alertIds[0] }, { $set: { Status: "Acknowledged" } }));

print("-- updateOne: resolve an alert and $push a ResolutionActions entry (field did not exist before) --");
printjson(db.alerts.updateOne(
  { _id: alertIds[1] },
  {
    $set: { Status: "Resolved", ResolvedAt: new Date() },
    $push: { ResolutionActions: { action: "Resolved", by: userIds.maria, at: new Date() } }
  }
));
print("resolved alert now has ResolutionActions:");
printjson(db.alerts.findOne({ _id: alertIds[1] }));

print("-- updateMany: deactivate every LeakDetector sensor for maintenance --");
printjson(db.sensors.updateMany({ Type: "LeakDetector" }, { $set: { IsActive: false } }));

print("-- updateOne: toggle a sensor's flexible Specs (add a field that did not exist) --");
printjson(db.sensors.updateOne({ _id: sensorIds.entrada }, { $set: { "Specs.lastMaintenanceAt": new Date() } }));

// =========================== DELETE ===========================
print("\n================ 9) DELETE examples ================");
print("-- deleteOne: remove a single stale reading --");
printjson(db.readings.deleteOne({ SensorId: sensorIds.caixaJoao, LitersPerHour: 25 }));

print("-- deleteMany: purge Low-severity alerts older than the demo window --");
printjson(db.alerts.deleteMany({ Severity: "Low" }));

print("\n================ FINAL COUNTS ================");
print("users:", db.users.countDocuments());
print("sensors:", db.sensors.countDocuments());
print("readings:", db.readings.countDocuments());
print("alerts:", db.alerts.countDocuments());
print("impact_reports:", db.impact_reports.countDocuments());
