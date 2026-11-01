-- Optional sample policies, the same ones the in-memory store starts with. Run after 002:
--   psql -h localhost -U insurance -d insurance -f database/postgres/003_seed_sample_data.sql
-- Safe to re-run: existing rows are left untouched.

INSERT INTO policies (id, policy_number, policy_holder_name, policy_holder_email, type, status,
                      effective_date, expiration_date, premium, coverage_amount, deductible,
                      created_at_utc, updated_at_utc)
VALUES
    ('00000000-0000-0000-0000-000000000001', 'POL-2025-000101', 'Aarav Sharma',    'aarav.sharma@example.com',   'Auto',   'Active',    '2025-11-01', '2026-11-01', 1150, 50000,   500,  '2026-01-01T00:00:00Z', '2026-01-01T00:00:00Z'),
    ('00000000-0000-0000-0000-000000000002', 'POL-2025-000102', 'Emily Johnson',   'emily.johnson@example.com',  'Home',   'Active',    '2025-06-15', '2026-06-15', 1820, 450000,  2500, '2026-01-01T00:00:00Z', '2026-01-01T00:00:00Z'),
    ('00000000-0000-0000-0000-000000000003', 'POL-2024-000103', 'Carlos Mendes',   'carlos.mendes@example.com',  'Life',   'Active',    '2024-03-01', '2044-03-01', 960,  500000,  0,    '2026-01-01T00:00:00Z', '2026-01-01T00:00:00Z'),
    ('00000000-0000-0000-0000-000000000004', 'POL-2025-000104', 'Priya Nair',      'priya.nair@example.com',     'Health', 'Active',    '2025-01-01', '2026-01-01', 4200, 1000000, 1500, '2026-01-01T00:00:00Z', '2026-01-01T00:00:00Z'),
    ('00000000-0000-0000-0000-000000000005', 'POL-2025-000105', 'Liam O''Connor',  'liam.oconnor@example.com',   'Travel', 'Expired',   '2025-07-10', '2025-08-10', 85,   25000,   100,  '2026-01-01T00:00:00Z', '2026-01-01T00:00:00Z'),
    ('00000000-0000-0000-0000-000000000006', 'POL-2024-000106', 'Sophie Martin',   'sophie.martin@example.com',  'Auto',   'Lapsed',    '2024-09-01', '2025-09-01', 980,  40000,   750,  '2026-01-01T00:00:00Z', '2026-01-01T00:00:00Z'),
    ('00000000-0000-0000-0000-000000000007', 'POL-2025-000107', 'Kenji Tanaka',    'kenji.tanaka@example.com',   'Home',   'Cancelled', '2025-02-01', '2026-02-01', 1430, 320000,  1000, '2026-01-01T00:00:00Z', '2026-01-01T00:00:00Z'),
    ('00000000-0000-0000-0000-000000000008', 'POL-2026-000108', 'Fatima Al-Sayed', 'fatima.alsayed@example.com', 'Health', 'Draft',     '2026-02-01', '2027-02-01', 3650, 750000,  2000, '2026-01-01T00:00:00Z', '2026-01-01T00:00:00Z'),
    ('00000000-0000-0000-0000-000000000009', 'POL-2025-000109', 'Noah Williams',   'noah.williams@example.com',  'Auto',   'Active',    '2025-08-20', '2026-02-20', 640,  30000,   1000, '2026-01-01T00:00:00Z', '2026-01-01T00:00:00Z'),
    ('00000000-0000-0000-0000-000000000010', 'POL-2025-000110', 'Isabella Rossi',  'isabella.rossi@example.com', 'Life',   'Active',    '2025-05-05', '2055-05-05', 1250, 750000,  0,    '2026-01-01T00:00:00Z', '2026-01-01T00:00:00Z'),
    ('00000000-0000-0000-0000-000000000011', 'POL-2026-000111', 'Aarav Sharma',    'aarav.sharma@example.com',   'Home',   'Draft',     '2026-03-01', '2027-03-01', 1560, 380000,  2000, '2026-01-01T00:00:00Z', '2026-01-01T00:00:00Z'),
    ('00000000-0000-0000-0000-000000000012', 'POL-2025-000112', 'Olivia Brown',    'olivia.brown@example.com',   'Travel', 'Active',    '2025-12-20', '2026-01-20', 120,  50000,   150,  '2026-01-01T00:00:00Z', '2026-01-01T00:00:00Z')
ON CONFLICT (id) DO NOTHING;
