-- =============================================================================
-- 05 — ZONES + PINCODES: tblZone, tblZonePincode
-- Hyderabad PLACEHOLDER zones and pincodes — replace with your real serviced
-- areas before launch. Pincode → zone is 1:1 here (GetZoneByPincode returns one
-- zone); add more rows to tblZonePincode to map additional pincodes to a zone.
-- Idempotent: zones on ZoneCode, pincodes on (ZoneId, Pincode).
-- =============================================================================
BEGIN;

-- ── Zones ────────────────────────────────────────────────────────────────────
INSERT INTO "tblZone"
  ("ZoneCode","ZoneName","CityName","IsActive",
   "CompanyId","SiteId","BranchId","DisplayOnWeb","IsPublished","SortOrder","IPAddress","CreatedBy","DateCreated","IsDeleted")
SELECT v.code, v.name, 'Hyderabad', true,
   1,1,1,true,true,v.sort,'127.0.0.1','System Seed',timezone('utc',now()),false
FROM (VALUES
  ('ZN-BANJARA',   'Banjara Hills',  1),
  ('ZN-JUBILEE',   'Jubilee Hills',  2),
  ('ZN-GACHIBOWLI','Gachibowli',     3),
  ('ZN-MADHAPUR',  'Madhapur',       4),
  ('ZN-HITECH',    'Hitech City',    5),
  ('ZN-KONDAPUR',  'Kondapur',       6),
  ('ZN-KUKATPALLY','Kukatpally',     7),
  ('ZN-SECBAD',    'Secunderabad',   8)
) AS v(code, name, sort)
WHERE NOT EXISTS (SELECT 1 FROM "tblZone" z WHERE z."ZoneCode" = v.code AND z."IsDeleted" = false);

-- ── Pincodes (FK to zone by ZoneCode) ────────────────────────────────────────
INSERT INTO "tblZonePincode"
  ("ZoneId","Pincode","IsActive",
   "CompanyId","SiteId","BranchId","DisplayOnWeb","IsPublished","SortOrder","IPAddress","CreatedBy","DateCreated","IsDeleted")
SELECT (SELECT z."ZoneId" FROM "tblZone" z WHERE z."ZoneCode" = v.zonecode AND z."IsDeleted" = false ORDER BY z."ZoneId" DESC LIMIT 1),
   v.pincode, true,
   1,1,1,true,true,0,'127.0.0.1','System Seed',timezone('utc',now()),false
FROM (VALUES
  ('ZN-BANJARA',   '500034'),
  ('ZN-JUBILEE',   '500033'),
  ('ZN-GACHIBOWLI','500032'),
  ('ZN-MADHAPUR',  '500081'),
  ('ZN-HITECH',    '500084'),
  ('ZN-KONDAPUR',  '500085'),  -- PLACEHOLDER (distinct); set the real Kondapur pincode(s) before launch
  ('ZN-KUKATPALLY','500072'),
  ('ZN-SECBAD',    '500003')
) AS v(zonecode, pincode)
WHERE EXISTS (SELECT 1 FROM "tblZone" z WHERE z."ZoneCode" = v.zonecode AND z."IsDeleted" = false)
AND NOT EXISTS (
  SELECT 1 FROM "tblZonePincode" zp
  JOIN "tblZone" z ON z."ZoneId" = zp."ZoneId"
  WHERE z."ZoneCode" = v.zonecode AND zp."Pincode" = v.pincode AND zp."IsDeleted" = false
);

COMMIT;
