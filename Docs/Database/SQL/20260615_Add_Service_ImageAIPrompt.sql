-- 2026-06-15
-- Service Catalog — add an optional AI image-generation prompt per service so an admin can store a
-- tuned, reusable prompt and re-generate the service image consistently (one click, no rework).
-- Adds tblService.ImageAIPrompt (NULL = admin UI shows a generated suggested prompt instead).
-- Aligns with Coolzo.Domain/Entities/Service.cs (ImageAIPrompt) and
-- Coolzo.Persistence/Configurations/ServiceConfiguration.cs (HasMaxLength(1024)).
-- Idempotent: safe to re-run.

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.tblService') AND name = N'ImageAIPrompt'
)
BEGIN
    ALTER TABLE dbo.tblService ADD ImageAIPrompt NVARCHAR(1024) NULL;
END
GO
