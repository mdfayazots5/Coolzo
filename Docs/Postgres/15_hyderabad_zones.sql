-- ============================================================================
-- Hyderabad Zones & Pincodes Seed
-- Created   : 09 Jun 2026
-- Audited   : 09 Jun 2026 — 5 unverified pincodes removed; 500040 (Moula Ali)
--             corrected from wrong 500013/500061; 500011/500060/500093 deleted.
-- Description: 8 service zones covering Greater Hyderabad.
--              46 pincodes — all confirmed against India Post records.
--              Zone IDs start at 3 (ZoneId 1,2 = existing Bengaluru zones).
--              ZonePincode IDs start at 5 (IDs 1–4 = existing Bengaluru data).
-- Run       : psql -U <user> -d <db> -f 15_hyderabad_zones.sql
-- ============================================================================

-- ── 8 Hyderabad Zones ───────────────────────────────────────────────────────

BEGIN;
INSERT INTO public."tblZone"
("ZoneId", "CompanyId", "SiteId", "DepartmentId",
 "ZoneCode", "ZoneName", "CityName",
 "IsActive", "Tag", "Comments", "DisplayOnWeb", "IsPublished",
 "DatePublished", "PublishedBy", "SortOrder", "IPAddress",
 "CreatedBy", "DateCreated", "UpdatedBy", "LastUpdated",
 "DeletedBy", "DateDeleted", "IsDeleted", "BranchId")
OVERRIDING SYSTEM VALUE
VALUES
-- Nampally, Abids, Secretariat, Himayatnagar, Narayanaguda,
-- Masab Tank, King Koti, Panjagutta, Musheerabad, Kachiguda
(3,  1, 1, NULL, 'HYD-CENTRAL',      'Central Hyderabad',
 'Hyderabad', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL,  3, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1),

-- Charminar, Laad Bazaar, Moghalpura, Malakpet, Falaknuma, Saidabad
(4,  1, 1, NULL, 'HYD-OLDCITY',      'Old City / Charminar',
 'Hyderabad', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL,  4, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1),

-- Secunderabad, Rasoolpura, Begumpet, Marredpally, Tarnaka,
-- Bowenpally, Trimulgherry Cantonment
(5,  1, 1, NULL, 'HYD-SECUNDERABAD', 'Secunderabad',
 'Hyderabad', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL,  5, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1),

-- Banjara Hills, Jubilee Hills, Shaikpet, Ameerpet,
-- Tolichowki, Mehdipatnam, Yousufguda
(6,  1, 1, NULL, 'HYD-WESTINNER',    'Banjara Hills / Jubilee Hills',
 'Hyderabad', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL,  6, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1),

-- Madhapur, HITEC City, Gachibowli, Kondapur,
-- Raidurgam, Narsingi, Kokapet, Nallagandla
(7,  1, 1, NULL, 'HYD-TECH',         'HITEC City / Madhapur / Gachibowli',
 'Hyderabad', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL,  7, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1),

-- Kukatpally, KPHB Colony, Miyapur, Chandanagar, Sanathnagar
(8,  1, 1, NULL, 'HYD-NORTHWEST',    'Kukatpally / KPHB / Miyapur',
 'Hyderabad', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL,  8, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1),

-- LB Nagar, Dilsukhnagar, Uppal, Vanasthalipuram,
-- Hayathnagar, Moula Ali
(9,  1, 1, NULL, 'HYD-EAST',         'LB Nagar / Dilsukhnagar / Uppal',
 'Hyderabad', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL,  9, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1),

-- Malkajgiri, Yapral, Neredmet, Sainikpuri, Alwal, Kompally
(10, 1, 1, NULL, 'HYD-NORTH',        'Malkajgiri / Alwal / Kompally',
 'Hyderabad', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 10, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1)

ON CONFLICT DO NOTHING;
COMMIT;


-- ── Pincode → Zone Mapping — 46 verified pincodes ───────────────────────────
-- Removed (unverified): 500011, 500013, 500060, 500061, 500093
-- Corrected: Moula Ali = 500040 (not 500013/500061)

BEGIN;
INSERT INTO public."tblZonePincode"
("ZonePincodeId", "CompanyId", "SiteId", "DepartmentId",
 "ZoneId", "Pincode",
 "IsActive", "Tag", "Comments", "DisplayOnWeb", "IsPublished",
 "DatePublished", "PublishedBy", "SortOrder", "IPAddress",
 "CreatedBy", "DateCreated", "UpdatedBy", "LastUpdated",
 "DeletedBy", "DateDeleted", "IsDeleted", "BranchId")
OVERRIDING SYSTEM VALUE
VALUES

-- ── Zone 3: HYD-CENTRAL — 8 pincodes ────────────────────────────────────────
( 5, 1, 1, NULL, 3, '500001', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Nampally / Abids
( 6, 1, 1, NULL, 3, '500004', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Secretariat / Basheerbagh
( 7, 1, 1, NULL, 3, '500019', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- King Koti / Basheer Bagh Hospital
( 8, 1, 1, NULL, 3, '500028', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Himayatnagar / Narayanaguda
( 9, 1, 1, NULL, 3, '500029', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Masab Tank
(10, 1, 1, NULL, 3, '500037', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Musheerabad / Nallakunta
(11, 1, 1, NULL, 3, '500038', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Kachiguda
(12, 1, 1, NULL, 3, '500063', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Panjagutta / Somajiguda

-- ── Zone 4: HYD-OLDCITY — 6 pincodes ────────────────────────────────────────
(13, 1, 1, NULL, 4, '500002', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Charminar
(14, 1, 1, NULL, 4, '500005', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Laad Bazaar / Sultan Bazaar
(15, 1, 1, NULL, 4, '500006', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Moghalpura / Begum Bazaar
(16, 1, 1, NULL, 4, '500053', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Malakpet
(17, 1, 1, NULL, 4, '500059', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Falaknuma
(18, 1, 1, NULL, 4, '500064', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Saidabad / Chandrayangutta

-- ── Zone 5: HYD-SECUNDERABAD — 7 pincodes ───────────────────────────────────
(19, 1, 1, NULL, 5, '500003', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Secunderabad H.O. / M.G. Road
(20, 1, 1, NULL, 5, '500007', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Rasoolpura / James Street
(21, 1, 1, NULL, 5, '500015', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Bowenpally
(22, 1, 1, NULL, 5, '500017', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Begumpet / S.P. Road
(23, 1, 1, NULL, 5, '500026', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Marredpally / West Maredpally
(24, 1, 1, NULL, 5, '500056', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Tarnaka / Osmania University
(25, 1, 1, NULL, 5, '500080', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Trimulgherry / Secunderabad Cantonment

-- ── Zone 6: HYD-WESTINNER — 6 pincodes ──────────────────────────────────────
(26, 1, 1, NULL, 6, '500008', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Tolichowki / Mehdipatnam
(27, 1, 1, NULL, 6, '500016', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Ameerpet / Erragadda
(28, 1, 1, NULL, 6, '500024', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Yousufguda
(29, 1, 1, NULL, 6, '500033', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Banjara Hills
(30, 1, 1, NULL, 6, '500034', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Jubilee Hills
(31, 1, 1, NULL, 6, '500082', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Shaikpet / Jubilee Hills Extension

-- ── Zone 7: HYD-TECH — 5 pincodes ───────────────────────────────────────────
(32, 1, 1, NULL, 7, '500032', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Gachibowli
(33, 1, 1, NULL, 7, '500049', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Kondapur
(34, 1, 1, NULL, 7, '500081', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Madhapur / HITEC City / Cyberabad
(35, 1, 1, NULL, 7, '500084', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Raidurgam / Narsingi / Kokapet
(36, 1, 1, NULL, 7, '500088', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Nallagandla

-- ── Zone 8: HYD-NORTHWEST — 4 pincodes ──────────────────────────────────────
-- Note: 500093 removed — no confirmed India Post record for Nizampet at 500093;
--       Nizampet is covered by the Kukatpally/KPHB postal circle (500085/500072)
(37, 1, 1, NULL, 8, '500018', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Sanathnagar
(38, 1, 1, NULL, 8, '500072', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Kukatpally
(39, 1, 1, NULL, 8, '500085', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- KPHB Colony
(40, 1, 1, NULL, 8, '500090', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Miyapur / Chandanagar / Bachupally

-- ── Zone 9: HYD-EAST — 6 pincodes ───────────────────────────────────────────
-- Note: 500013 removed (wrong — not Moula Ali); 500060 removed (Kapra is 501xxx
--       Medchal range); 500061 removed (conflicting Moula Ali claim).
--       500040 added as the confirmed India Post pincode for Moula Ali.
(41, 1, 1, NULL, 9, '500035', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- LB Nagar
(42, 1, 1, NULL, 9, '500036', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Dilsukhnagar
(43, 1, 1, NULL, 9, '500039', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Hayathnagar
(44, 1, 1, NULL, 9, '500040', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Moula Ali (confirmed India Post)
(45, 1, 1, NULL, 9, '500068', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Uppal
(46, 1, 1, NULL, 9, '500070', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Vanasthalipuram

-- ── Zone 10: HYD-NORTH — 4 pincodes ─────────────────────────────────────────
-- Note: 500011 removed — unconfirmed for Quthbullapur; Quthbullapur is in
--       Medchal district (501xxx range), not inner Hyderabad 500xxx.
(47, 1, 1, NULL, 10, '500010', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Neredmet / Sainikpuri
(48, 1, 1, NULL, 10, '500014', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Malkajgiri / Yapral
(49, 1, 1, NULL, 10, '500067', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1), -- Alwal
(50, 1, 1, NULL, 10, '500100', TRUE, NULL, NULL, TRUE, TRUE, NULL, NULL, 0, '127.0.0.1', 'HydSeed', NOW(), NULL, NULL, NULL, NULL, FALSE, 1)  -- Kompally

ON CONFLICT DO NOTHING;
COMMIT;


-- ── Reset sequences so EF Core identity columns continue from correct value ──

SELECT setval(
    pg_get_serial_sequence('public."tblZone"', 'ZoneId'),
    (SELECT MAX("ZoneId") FROM public."tblZone")
);

SELECT setval(
    pg_get_serial_sequence('public."tblZonePincode"', 'ZonePincodeId'),
    (SELECT MAX("ZonePincodeId") FROM public."tblZonePincode")
);
