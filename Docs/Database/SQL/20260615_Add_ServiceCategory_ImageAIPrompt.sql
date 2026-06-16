-- 2026-06-15
-- Service Catalog — add an optional AI image-generation prompt per CATEGORY so an admin can store a
-- tuned, reusable prompt and re-generate the category image consistently (one click, no rework).
-- Mirrors the per-service prompt. Adds tblServiceCategory.ImageAIPrompt (NULL = admin UI shows a
-- generated suggested prompt instead).
-- Aligns with Coolzo.Domain/Entities/ServiceCategory.cs (ImageAIPrompt) and
-- Coolzo.Persistence/Configurations/ServiceCategoryConfiguration.cs (HasMaxLength(1024)).
-- Idempotent: safe to re-run.

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.tblServiceCategory') AND name = N'ImageAIPrompt'
)
BEGIN
    ALTER TABLE dbo.tblServiceCategory ADD ImageAIPrompt NVARCHAR(1024) NULL;
END
GO
