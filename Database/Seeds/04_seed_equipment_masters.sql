-- =============================================================================
-- 04 — EQUIPMENT MASTERS: tblAcType, tblTonnage, tblBrand
-- AcType is REQUIRED in the booking wizard; Tonnage/Brand are optional
-- (technician confirms on-site) but seeded so the data is available.
-- Idempotent on each *Code.
-- =============================================================================
BEGIN;

-- ── AC types ────────────────────────────────────────────────────────────────
INSERT INTO "tblAcType"
  ("AcTypeCode","AcTypeName","Description","IsActive",
   "CompanyId","SiteId","BranchId","DisplayOnWeb","IsPublished","SortOrder","IPAddress","CreatedBy","DateCreated","IsDeleted")
SELECT v.code, v.name, v.descr, true,
   1,1,1,true,true,v.sort,'127.0.0.1','System Seed',timezone('utc',now()),false
FROM (VALUES
  ('ACT-SPLIT',    'Split AC',                 'Wall-mounted split air conditioner.', 1),
  ('ACT-WINDOW',   'Window AC',                'Window-mounted air conditioner.',     2),
  ('ACT-CASSETTE', 'Cassette AC',              'Ceiling cassette air conditioner.',   3),
  ('ACT-TOWER',    'Tower / Floor-standing AC','Floor-standing tower air conditioner.',4),
  ('ACT-DUCT',     'Ducted AC',                'Concealed ducted air conditioner.',   5)
) AS v(code, name, descr, sort)
WHERE NOT EXISTS (SELECT 1 FROM "tblAcType" a WHERE a."AcTypeCode" = v.code AND a."IsDeleted" = false);

-- ── Tonnages ─────────────────────────────────────────────────────────────────
INSERT INTO "tblTonnage"
  ("TonnageCode","TonnageName","Description","IsActive",
   "CompanyId","SiteId","BranchId","DisplayOnWeb","IsPublished","SortOrder","IPAddress","CreatedBy","DateCreated","IsDeleted")
SELECT v.code, v.name, v.descr, true,
   1,1,1,true,true,v.sort,'127.0.0.1','System Seed',timezone('utc',now()),false
FROM (VALUES
  ('TON-10', '1.0 Ton', 'Suitable for small rooms (up to ~120 sq ft).', 1),
  ('TON-15', '1.5 Ton', 'Suitable for medium rooms (~120-180 sq ft).',  2),
  ('TON-20', '2.0 Ton', 'Suitable for large rooms (~180-250 sq ft).',   3),
  ('TON-30', '3.0 Ton', 'Suitable for halls / commercial spaces.',      4)
) AS v(code, name, descr, sort)
WHERE NOT EXISTS (SELECT 1 FROM "tblTonnage" t WHERE t."TonnageCode" = v.code AND t."IsDeleted" = false);

-- ── Brands ───────────────────────────────────────────────────────────────────
INSERT INTO "tblBrand"
  ("BrandCode","BrandName","Description","IsActive",
   "CompanyId","SiteId","BranchId","DisplayOnWeb","IsPublished","SortOrder","IPAddress","CreatedBy","DateCreated","IsDeleted")
SELECT v.code, v.name, v.descr, true,
   1,1,1,true,true,v.sort,'127.0.0.1','System Seed',timezone('utc',now()),false
FROM (VALUES
  ('BR-DAIKIN',    'Daikin',    'Daikin air conditioners.',    1),
  ('BR-VOLTAS',    'Voltas',    'Voltas air conditioners.',    2),
  ('BR-LG',        'LG',        'LG air conditioners.',        3),
  ('BR-SAMSUNG',   'Samsung',   'Samsung air conditioners.',   4),
  ('BR-BLUESTAR',  'Blue Star', 'Blue Star air conditioners.', 5),
  ('BR-HITACHI',   'Hitachi',   'Hitachi air conditioners.',   6),
  ('BR-PANASONIC', 'Panasonic', 'Panasonic air conditioners.', 7),
  ('BR-CARRIER',   'Carrier',   'Carrier air conditioners.',   8),
  ('BR-OTHER',     'Other',     'Any other / unlisted brand.', 9)
) AS v(code, name, descr, sort)
WHERE NOT EXISTS (SELECT 1 FROM "tblBrand" b WHERE b."BrandCode" = v.code AND b."IsDeleted" = false);

COMMIT;
