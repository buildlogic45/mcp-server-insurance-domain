-- Creates the schema. Run as the application role, connected to the insurance database, so it owns
-- the tables:
--   psql -h localhost -U insurance -d insurance -f database/postgres/002_create_tables.sql
-- Safe to re-run. After changing this script, re-scaffold the EF Core model (see README).

CREATE TABLE IF NOT EXISTS policies (
    id                  uuid                    NOT NULL,
    -- "C" collation sorts byte-wise, matching the ordinal ordering of the in-memory store.
    policy_number       varchar(32) COLLATE "C" NOT NULL,
    policy_holder_name  varchar(200)            NOT NULL,
    policy_holder_email varchar(320)            NOT NULL,
    type                varchar(16)             NOT NULL,
    status              varchar(16)             NOT NULL,
    effective_date      date                    NOT NULL,
    expiration_date     date                    NOT NULL,
    premium             numeric(14, 2)          NOT NULL,
    coverage_amount     numeric(14, 2)          NOT NULL,
    deductible          numeric(14, 2)          NOT NULL DEFAULT 0,
    created_at_utc      timestamptz             NOT NULL,
    updated_at_utc      timestamptz             NOT NULL,

    CONSTRAINT pk_policies PRIMARY KEY (id),
    CONSTRAINT uq_policies_policy_number UNIQUE (policy_number),
    -- Values must match the PolicyType and PolicyStatus enums in Core.
    CONSTRAINT ck_policies_type CHECK (type IN ('Auto', 'Home', 'Life', 'Health', 'Travel')),
    CONSTRAINT ck_policies_status CHECK (status IN ('Draft', 'Active', 'Lapsed', 'Cancelled', 'Expired')),
    CONSTRAINT ck_policies_dates CHECK (expiration_date > effective_date),
    CONSTRAINT ck_policies_amounts CHECK (premium > 0 AND coverage_amount > 0 AND deductible BETWEEN 0 AND coverage_amount)
);

CREATE INDEX IF NOT EXISTS ix_policies_type_status ON policies (type, status);
CREATE INDEX IF NOT EXISTS ix_policies_effective_date ON policies (effective_date);
