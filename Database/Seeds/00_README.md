# Phase 3 — Master Data Seed (Supabase Postgres)
**Status:** FOR REVIEW — do NOT run until approved. **Created:** 2026-06-10 (Website Rework Phase 3).
**Target:** Production Supabase Postgres. **Data:** realistic Hyderabad PLACEHOLDERS — review/replace before launch.

> These scripts are NOT auto-executed. Review, edit the placeholder values, then run in order in the
> Supabase SQL editor. Each file is wrapped in BEGIN/COMMIT and is idempotent (safe to re-run).

## Why explicit audit columns
The EF model sets `DateCreated` default to `GETUTCDATE()` (a SQL-Server function). On Postgres that
default is invalid, so every INSERT here sets the NOT-NULL audit columns explicitly
(`CompanyId/SiteId/BranchId/DisplayOnWeb/IsPublished/SortOrder/IPAddress/CreatedBy/DateCreated/IsDeleted`).
Identifiers are double-quoted because EF created the tables with PascalCase names (case-sensitive in PG).

## Run order
| # | File | Seeds |
|---|------|-------|
| 01 | `01_purge_test_masters.sql` | SOFT-deletes all existing master rows (sets IsDeleted=true). Non-destructive. Hard-delete variant included, commented out. |
| 02 | `02_seed_pricing_and_categories.sql` | `tblPricingModel`, `tblServiceCategory` |
| 03 | `03_seed_services.sql` | `tblService` (FK → category by CategoryCode, pricing model by name) |
| 04 | `04_seed_equipment_masters.sql` | `tblAcType`, `tblTonnage`, `tblBrand` |
| 05 | `05_seed_zones_and_pincodes.sql` | `tblZone`, `tblZonePincode` |
| 06 | `06_seed_slots.sql` | `tblSlotConfiguration` (3 windows/zone) + `tblSlotAvailability` (next 14 days, all zones). Re-run after adding zones (07/08) so new zones get slots. |
| 07 | `07_seed_more_zones_and_pincodes.sql` | 11 more named Hyderabad zones + real pincodes (approximate mapping — verify) |
| 08 | `08_seed_all_hyderabad_pincodes.sql` | "Hyderabad (City-wide)" catch-all zone + every 500001-500113 pincode not already mapped |
| 09 | `09_configure_booking_flags.sql` | `tblSystemSetting`: Booking.OpenBookingMode=false, Booking.EnforceSlotCapacity=true (NOTE: this table has NO BranchId column) |

Order with the expansion: 01 → 02 → 03 → 04 → 05 → 07 → 08 → 06 → 09 (06 last so it picks up all zones).

## Decisions baked in (confirmed 2026-06-10)
- Data = realistic Hyderabad placeholders (replace prices/zones/pincodes with real values before launch).
- Slots = Morning (08–12) / Afternoon (12–16) / Evening (16–19), next 14 days, every zone, capacity 5.

## Placeholders to replace before launch
- Service `BasePrice` values (₹) — set your real pricing.
- Zone list + `tblZonePincode` pincodes — set your real serviced pincodes.
- Slot capacity (currently `MaxBookingCount = 5`) — set per your technician capacity.

## Rollback
Re-running `01` re-soft-deletes; to restore a soft-deleted set, `UPDATE ... SET "IsDeleted"=false`.
A hard-delete (destructive) block is provided commented-out in `01` for a true clean slate (needs explicit approval).
