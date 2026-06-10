-- 2026-06-10
-- Phase 1 — Catalog / public-site service reconciliation: per-service image.
-- Adds tblService.ImageUrl (object-storage public URL; NULL = UI fallback).
-- Aligns with Coolzo.Domain/Entities/Service.cs (ImageUrl) and
-- Coolzo.Persistence/Configurations/ServiceConfiguration.cs (HasMaxLength(512)).
-- Idempotent: safe to re-run.

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.tblService') AND name = N'ImageUrl'
)
BEGIN
    ALTER TABLE dbo.tblService ADD ImageUrl NVARCHAR(512) NULL;
END
GO
