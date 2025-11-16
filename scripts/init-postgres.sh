#!/bin/bash
set -e

# Create sonarqube database if it doesn't exist
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
    SELECT 'CREATE DATABASE sonarqube'
    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'sonarqube')\gexec
EOSQL

echo "PostgreSQL initialization complete: CMS_DB and sonarqube databases created"
