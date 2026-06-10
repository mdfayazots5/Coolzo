-- =============================================================================
-- 07 — EXPANDED HYDERABAD ZONES + PINCODES (additive)
-- Adds more major Hyderabad zones and a broader set of REAL pincodes, each
-- mapped to a best-fit zone. Pincode->zone is approximate PLACEHOLDER (Hyderabad
-- pincodes overlap localities) — VERIFY/adjust against your real serviced areas.
-- After this, re-run 06_seed_slots.sql to create slots for the new zones.
-- Idempotent. Requires 05 to have run.
-- =============================================================================
BEGIN;

-- ── New zones ────────────────────────────────────────────────────────────────
INSERT INTO "tblZone"
  ("ZoneCode","ZoneName","CityName","IsActive",
   "CompanyId","SiteId","BranchId","DisplayOnWeb","IsPublished","SortOrder","IPAddress","CreatedBy","DateCreated","IsDeleted")
SELECT v.code, v.name, 'Hyderabad', true,
   1,1,1,true,true,v.sort,'127.0.0.1','System Seed',timezone('utc',now()),false
FROM (VALUES
  ('ZN-AMEERPET',    'Ameerpet',           20),
  ('ZN-BEGUMPET',    'Begumpet',           21),
  ('ZN-MEHDIPATNAM', 'Mehdipatnam',        22),
  ('ZN-MIYAPUR',     'Miyapur',            23),
  ('ZN-MANIKONDA',   'Manikonda / Narsingi',24),
  ('ZN-DILSUKHNAGAR','Dilsukhnagar',       25),
  ('ZN-LBNAGAR',     'LB Nagar',           26),
  ('ZN-UPPAL',       'Uppal',              27),
  ('ZN-KOMPALLY',    'Kompally',           28),
  ('ZN-ABIDS',       'Abids / Koti',       29),
  ('ZN-CHARMINAR',   'Charminar / Old City',30)
) AS v(code, name, sort)
WHERE NOT EXISTS (SELECT 1 FROM "tblZone" z WHERE z."ZoneCode" = v.code AND z."IsDeleted" = false);

-- ── Pincodes (each maps to exactly one zone; all values unique) ──────────────
INSERT INTO "tblZonePincode"
  ("ZoneId","Pincode","IsActive",
   "CompanyId","SiteId","BranchId","DisplayOnWeb","IsPublished","SortOrder","IPAddress","CreatedBy","DateCreated","IsDeleted")
SELECT (SELECT z."ZoneId" FROM "tblZone" z WHERE z."ZoneCode" = v.zonecode AND z."IsDeleted" = false ORDER BY z."ZoneId" DESC LIMIT 1),
   v.pincode, true,
   1,1,1,true,true,0,'127.0.0.1','System Seed',timezone('utc',now()),false
FROM (VALUES
  -- additional pincodes for EXISTING zones
  ('ZN-BANJARA',    '500004'), ('ZN-BANJARA',    '500082'),
  ('ZN-JUBILEE',    '500045'), ('ZN-JUBILEE',    '500096'),
  ('ZN-GACHIBOWLI', '500046'), ('ZN-GACHIBOWLI', '500075'),
  ('ZN-KUKATPALLY', '500049'), ('ZN-KUKATPALLY', '500090'),
  ('ZN-SECBAD',     '500009'), ('ZN-SECBAD',     '500025'), ('ZN-SECBAD','500044'), ('ZN-SECBAD','500080'),
  -- new zones
  ('ZN-AMEERPET',    '500016'), ('ZN-AMEERPET',    '500038'),
  ('ZN-BEGUMPET',    '500061'),
  ('ZN-MEHDIPATNAM', '500008'), ('ZN-MEHDIPATNAM', '500028'),
  ('ZN-MIYAPUR',     '500050'),
  ('ZN-MANIKONDA',   '500089'),
  ('ZN-DILSUKHNAGAR','500060'), ('ZN-DILSUKHNAGAR','500035'),
  ('ZN-LBNAGAR',     '500070'), ('ZN-LBNAGAR',     '500074'),
  ('ZN-UPPAL',       '500039'), ('ZN-UPPAL',       '500036'),
  ('ZN-KOMPALLY',    '500100'),
  ('ZN-ABIDS',       '500001'),
  ('ZN-CHARMINAR',   '500002')
) AS v(zonecode, pincode)
WHERE EXISTS (SELECT 1 FROM "tblZone" z WHERE z."ZoneCode" = v.zonecode AND z."IsDeleted" = false)
AND NOT EXISTS (
  SELECT 1 FROM "tblZonePincode" zp
  JOIN "tblZone" z ON z."ZoneId" = zp."ZoneId"
  WHERE z."ZoneCode" = v.zonecode AND zp."Pincode" = v.pincode AND zp."IsDeleted" = false
);

COMMIT;
