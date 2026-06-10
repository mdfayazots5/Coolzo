-- =============================================================================
-- 08 — ALL HYDERABAD PINCODES (catch-all so no pincode is left unserviceable)
-- Adds a "Hyderabad (City-wide)" zone and maps EVERY core Hyderabad pincode
-- (500001-500113) to it, EXCEPT pincodes already mapped to a specific zone in
-- files 05/07 (those keep their specific zone). Guarantees pincode lookup never
-- fails for a Hyderabad customer. Re-run 06 afterwards so this zone has slots.
-- Idempotent.
-- NOTE: 500001-500113 is the GHMC core. If you also serve outskirts (501xxx/502xxx),
--       add those ranges the same way.
-- =============================================================================
BEGIN;

-- ── City-wide catch-all zone ─────────────────────────────────────────────────
INSERT INTO "tblZone"
  ("ZoneCode","ZoneName","CityName","IsActive",
   "CompanyId","SiteId","BranchId","DisplayOnWeb","IsPublished","SortOrder","IPAddress","CreatedBy","DateCreated","IsDeleted")
SELECT 'ZN-HYD-CITY','Hyderabad (City-wide)','Hyderabad', true,
   1,1,1,true,true,99,'127.0.0.1','System Seed',timezone('utc',now()),false
WHERE NOT EXISTS (SELECT 1 FROM "tblZone" z WHERE z."ZoneCode" = 'ZN-HYD-CITY' AND z."IsDeleted" = false);

-- ── Every 500001-500113 pincode not already mapped → city-wide zone ──────────
INSERT INTO "tblZonePincode"
  ("ZoneId","Pincode","IsActive",
   "CompanyId","SiteId","BranchId","DisplayOnWeb","IsPublished","SortOrder","IPAddress","CreatedBy","DateCreated","IsDeleted")
SELECT (SELECT z."ZoneId" FROM "tblZone" z WHERE z."ZoneCode" = 'ZN-HYD-CITY' AND z."IsDeleted" = false ORDER BY z."ZoneId" DESC LIMIT 1),
   p.pincode, true,
   1,1,1,true,true,0,'127.0.0.1','System Seed',timezone('utc',now()),false
FROM (SELECT '500' || lpad(g::text, 3, '0') AS pincode FROM generate_series(1, 113) AS g) AS p
WHERE NOT EXISTS (
  SELECT 1 FROM "tblZonePincode" zp WHERE zp."Pincode" = p.pincode AND zp."IsDeleted" = false
);

COMMIT;
