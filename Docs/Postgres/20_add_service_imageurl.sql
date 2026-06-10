-- 2026-06-10
-- Phase 1 — Catalog / public-site service reconciliation: per-service image.
-- Adds tblService."ImageUrl" (object-storage public URL; NULL = UI fallback).
-- Aligns with Coolzo.Domain/Entities/Service.cs + ServiceConfiguration.cs (max length 512).
-- Idempotent: safe to re-run.

ALTER TABLE public."tblService"
    ADD COLUMN IF NOT EXISTS "ImageUrl" VARCHAR(512) NULL;
