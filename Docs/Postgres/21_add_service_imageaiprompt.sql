-- 2026-06-15
-- Service Catalog — add an optional AI image-generation prompt per service so an admin can store a
-- tuned, reusable prompt and re-generate the service image consistently (one click, no rework).
-- Adds tblService."ImageAIPrompt" (NULL = admin UI shows a generated suggested prompt instead).
-- Aligns with Coolzo.Domain/Entities/Service.cs + ServiceConfiguration.cs (max length 1024).
-- Idempotent: safe to re-run. PostgreSQL/Supabase (live prod DB).

ALTER TABLE public."tblService"
    ADD COLUMN IF NOT EXISTS "ImageAIPrompt" VARCHAR(1024) NULL;
