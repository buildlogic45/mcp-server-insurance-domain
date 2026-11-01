-- Creates the application role and database. Run once as a superuser, connected to any database:
--   psql -h localhost -U postgres -d postgres -f database/postgres/001_create_database.sql
-- Uses psql's \gexec, so it must be run with psql. Safe to re-run.
-- The password is for local development only; change it everywhere else.

SELECT 'CREATE ROLE insurance LOGIN PASSWORD ''insurance'''
WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'insurance')
\gexec

SELECT 'CREATE DATABASE insurance OWNER insurance ENCODING ''UTF8'''
WHERE NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = 'insurance')
\gexec
