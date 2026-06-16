-- 2026-06-15
-- Service Catalog — add an optional AI image-generation prompt per CATEGORY so an admin can store a
-- tuned, reusable prompt and re-generate the category image consistently (one click, no rework).
-- Mirrors the per-service prompt (migration 21). Adds tblServiceCategory."ImageAIPrompt"
-- (NULL = admin UI shows a generated suggested prompt instead).
-- Aligns with Coolzo.Domain/Entities/ServiceCategory.cs + ServiceCategoryConfiguration.cs (max length 1024).
-- Idempotent: safe to re-run. PostgreSQL/Supabase (live prod DB).

ALTER TABLE public."tblServiceCategory"
    ADD COLUMN IF NOT EXISTS "ImageAIPrompt" VARCHAR(1024) NULL;
