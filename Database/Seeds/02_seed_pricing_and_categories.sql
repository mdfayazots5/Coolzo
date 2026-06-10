-- =============================================================================
-- 02 — PRICING MODELS + SERVICE CATEGORIES
-- tblPricingModel (FK parent for tblService) and tblServiceCategory.
-- Idempotent: guarded by NOT EXISTS on the natural key.
-- =============================================================================
BEGIN;

-- ── Pricing models (referenced by services in file 03) ──────────────────────
INSERT INTO "tblPricingModel"
  ("PricingModelName","Description","BasePrice","IsActive",
   "CompanyId","SiteId","BranchId","DisplayOnWeb","IsPublished","SortOrder","IPAddress","CreatedBy","DateCreated","IsDeleted")
SELECT v.name, v.descr, v.price, true,
   1,1,1,true,true,v.sort,'127.0.0.1','System Seed',timezone('utc',now()),false
FROM (VALUES
  ('Fixed Price',       'Flat, upfront price for this service.',                     0::numeric, 1),
  ('Inspection-Based',  'Final price confirmed after on-site technician inspection.', 0::numeric, 2)
) AS v(name, descr, price, sort)
WHERE NOT EXISTS (
  SELECT 1 FROM "tblPricingModel" p WHERE p."PricingModelName" = v.name AND p."IsDeleted" = false
);

-- ── Service categories ──────────────────────────────────────────────────────
INSERT INTO "tblServiceCategory"
  ("CategoryCode","CategoryName","Description","IsActive",
   "CompanyId","SiteId","BranchId","DisplayOnWeb","IsPublished","SortOrder","IPAddress","CreatedBy","DateCreated","IsDeleted")
SELECT v.code, v.name, v.descr, true,
   1,1,1,true,true,v.sort,'127.0.0.1','System Seed',timezone('utc',now()),false
FROM (VALUES
  ('CAT-REPAIR',  'AC Repair',                    'Fast diagnosis and fix for any cooling fault.',      1),
  ('CAT-SERVICE', 'Service & Cleaning',           'Deep jet-wash and routine servicing for healthy air.', 2),
  ('CAT-INSTALL', 'Installation & Uninstallation','Safe, correct fitting and removal of AC units.',     3),
  ('CAT-GAS',     'Gas Refill',                   'Pressure check, leak fix and refrigerant top-up.',   4),
  ('CAT-AMC',     'AMC Plans',                    'Annual maintenance contracts with priority service.',5)
) AS v(code, name, descr, sort)
WHERE NOT EXISTS (
  SELECT 1 FROM "tblServiceCategory" c WHERE c."CategoryCode" = v.code AND c."IsDeleted" = false
);

COMMIT;
