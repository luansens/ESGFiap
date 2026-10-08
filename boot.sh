#!/bin/bash

set -e

echo "=================================="
echo "🚀 Inicialização do Projeto ESGFiap"
echo "=================================="
echo ""
echo "Selecione o modo de execução:"
echo "1) Desenvolvimento (dev)"
echo "2) Homologação (staging)"
echo "3) Produção (prod)"
echo ""
read -p "Digite 1, 2 ou 3: " MODE

export COMPOSE_PROJECT_NAME=esgfiap

case $MODE in
  1)
    echo ""
    echo "🧪 Iniciando em modo DESENVOLVIMENTO..."
    docker compose \
      -f docker-compose.yml \
      up
    ;;
  2)
    echo ""
    echo "🧷 Iniciando em modo HOMOLOGAÇÃO (staging)..."
    docker compose \
      -f docker-compose.staging.yml \
      build --no-cache
    docker compose \
      -f docker-compose.staging.yml \
      up -d
    ;;
  3)
    echo ""
    echo "🚀 Iniciando em modo PRODUÇÃO..."
    docker compose \
      -f docker-compose.prod.yml \
      build --no-cache
    docker compose \
      -f docker-compose.prod.yml \
      up -d
    ;;
  *)
    echo ""
    echo "❌ Opção inválida. Use 1 (dev), 2 (staging) ou 3 (prod)."
    exit 1
    ;;
esac
