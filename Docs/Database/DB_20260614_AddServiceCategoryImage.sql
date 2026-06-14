SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-------------------------------------------------------------------------------------------------------------
-- Created By   : Coolzo System
-- Date Created : 14 Jun 2026
-- Description  : Service Catalog — add optional ImageUrl to tblServiceCategory so an admin can set a
--                per-category image (used by the public Home catalog cards). Idempotent.
-------------------------------------------------------------------------------------------------------------

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.tblServiceCategory') AND name = N'ImageUrl'
)
BEGIN
    ALTER TABLE dbo.tblServiceCategory ADD ImageUrl NVARCHAR(512) NULL;
END
GO
