-- =============================================================================
-- 03 — SERVICES (tblService)
-- FK to category (by CategoryCode) and pricing model (by name) resolved via
-- subselect so generated identity IDs don't need to be known.
-- BasePrice values are PLACEHOLDERS (₹) — replace with real pricing before launch.
-- Requires files 02 to have run. Idempotent on ServiceCode (also a UNIQUE index).
-- =============================================================================
BEGIN;

INSERT INTO "tblService"
  ("ServiceCode","ServiceName","Summary","ImageUrl","EstimatedDurationInMinutes","BasePrice","IsActive",
   "ServiceCategoryId","PricingModelId",
   "CompanyId","SiteId","BranchId","DisplayOnWeb","IsPublished","SortOrder","IPAddress","CreatedBy","DateCreated","IsDeleted")
SELECT v.code, v.name, v.summary, NULL, 60, v.price, true,
   (SELECT c."ServiceCategoryId" FROM "tblServiceCategory" c WHERE c."CategoryCode" = v.catcode AND c."IsDeleted" = false ORDER BY c."ServiceCategoryId" DESC LIMIT 1),
   (SELECT p."PricingModelId"   FROM "tblPricingModel"   p WHERE p."PricingModelName" = v.pm     AND p."IsDeleted" = false ORDER BY p."PricingModelId" DESC LIMIT 1),
   1,1,1,true,true,v.sort,'127.0.0.1','System Seed',timezone('utc',now()),false
FROM (VALUES
  -- code            name                          summary                                                       price    catcode        pricing model        sort
  ('REP-SPLIT',      'Split AC Repair',            'Repair for split AC cooling, noise or power faults.',        299::numeric, 'CAT-REPAIR',  'Inspection-Based', 1),
  ('REP-WINDOW',     'Window AC Repair',           'Repair for window AC units of any brand.',                   299::numeric, 'CAT-REPAIR',  'Inspection-Based', 2),
  ('REP-NOCOOL',     'AC Not Cooling Diagnosis',   'On-site diagnosis when your AC runs but does not cool.',     199::numeric, 'CAT-REPAIR',  'Inspection-Based', 3),
  ('SVC-JET',        'Jet Service / Deep Clean',   'High-pressure jet cleaning of coils and filters.',           499::numeric, 'CAT-SERVICE', 'Fixed Price',      4),
  ('SVC-REG',        'Regular Service',            'Routine servicing to keep your AC running efficiently.',     399::numeric, 'CAT-SERVICE', 'Fixed Price',      5),
  ('INS-SPLIT',      'Split AC Installation',      'Professional installation of a new split AC.',               1499::numeric,'CAT-INSTALL', 'Fixed Price',      6),
  ('INS-UNINSTALL',  'AC Uninstallation',          'Safe removal and packing of an existing AC unit.',           699::numeric, 'CAT-INSTALL', 'Fixed Price',      7),
  ('GAS-REFILL',     'Gas Refill (Top-up)',        'Refrigerant top-up to restore full cooling.',                1999::numeric,'CAT-GAS',     'Inspection-Based', 8),
  ('GAS-LEAK',       'Gas Leak Fix + Refill',      'Leak detection, repair and full refrigerant refill.',        2499::numeric,'CAT-GAS',     'Inspection-Based', 9),
  ('AMC-BASIC',      'Basic AMC (2 visits/yr)',    'Two preventive maintenance visits per year.',                1999::numeric,'CAT-AMC',     'Fixed Price',      10),
  ('AMC-COMP',       'Comprehensive AMC',          'Priority support with parts cover and regular visits.',      3499::numeric,'CAT-AMC',     'Fixed Price',      11)
) AS v(code, name, summary, price, catcode, pm, sort)
WHERE NOT EXISTS (
  SELECT 1 FROM "tblService" s WHERE s."ServiceCode" = v.code AND s."IsDeleted" = false
);

COMMIT;
