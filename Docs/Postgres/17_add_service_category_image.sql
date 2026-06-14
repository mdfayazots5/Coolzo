-- 2026-06-14
-- Service Catalog: add an optional ImageUrl to service categories so an admin can set a per-category
-- image (used by the public Home catalog cards). Idempotent. PostgreSQL/Supabase.

ALTER TABLE public."tblServiceCategory"
    ADD COLUMN IF NOT EXISTS "ImageUrl" VARCHAR(512) NULL;
