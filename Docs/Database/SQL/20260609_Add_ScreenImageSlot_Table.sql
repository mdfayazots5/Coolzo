-- 2026-06-09
-- CMS Content & Theme Delivery (Phase 2) — admin-managed screen image slots.
-- Aligns with Backend/Coolzo.Domain/Entities/ScreenImageSlot.cs and
-- Backend/Coolzo.Persistence/Configurations/ScreenImageSlotConfiguration.cs
-- Table: tblScreenImageSlot

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tblScreenImageSlot') AND type = N'U')
BEGIN
    CREATE TABLE dbo.tblScreenImageSlot
    (
        ScreenImageSlotId   BIGINT IDENTITY(1,1) NOT NULL,
        CompanyId           INT NOT NULL CONSTRAINT DF_tblScreenImageSlot_CompanyId DEFAULT (1),
        SiteId              INT NOT NULL CONSTRAINT DF_tblScreenImageSlot_SiteId DEFAULT (1),
        BranchId            INT NOT NULL CONSTRAINT DF_tblScreenImageSlot_BranchId DEFAULT (1),
        DepartmentId        INT NULL,

        PageKey             NVARCHAR(64) NOT NULL,
        SlotKey             NVARCHAR(64) NOT NULL,
        Breakpoint          NVARCHAR(16) NOT NULL,
        RecommendedWidth    INT NOT NULL CONSTRAINT DF_tblScreenImageSlot_RecommendedWidth DEFAULT (0),
        RecommendedHeight   INT NOT NULL CONSTRAINT DF_tblScreenImageSlot_RecommendedHeight DEFAULT (0),
        AltText             NVARCHAR(256) NOT NULL CONSTRAINT DF_tblScreenImageSlot_AltText DEFAULT (N''),
        SuggestedAIPrompt   NVARCHAR(1024) NOT NULL CONSTRAINT DF_tblScreenImageSlot_SuggestedAIPrompt DEFAULT (N''),
        ImageUrl            NVARCHAR(512) NOT NULL CONSTRAINT DF_tblScreenImageSlot_ImageUrl DEFAULT (N''),
        IsActive            BIT NOT NULL CONSTRAINT DF_tblScreenImageSlot_IsActive DEFAULT (1),

        Tag                 NVARCHAR(64) NULL,
        Comments            NVARCHAR(512) NULL,
        DisplayOnWeb        BIT NOT NULL CONSTRAINT DF_tblScreenImageSlot_DisplayOnWeb DEFAULT (1),
        IsPublished         BIT NOT NULL CONSTRAINT DF_tblScreenImageSlot_IsPublished DEFAULT (1),
        DatePublished       DATETIME2 NULL,
        PublishedBy         NVARCHAR(128) NULL,
        SortOrder           INT NOT NULL CONSTRAINT DF_tblScreenImageSlot_SortOrder DEFAULT (0),
        IPAddress           NVARCHAR(64) NOT NULL CONSTRAINT DF_tblScreenImageSlot_IPAddress DEFAULT (N'127.0.0.1'),
        CreatedBy           NVARCHAR(128) NOT NULL CONSTRAINT DF_tblScreenImageSlot_CreatedBy DEFAULT (N'System'),
        DateCreated         DATETIME2 NOT NULL CONSTRAINT DF_tblScreenImageSlot_DateCreated DEFAULT (GETUTCDATE()),
        UpdatedBy           NVARCHAR(128) NULL,
        LastUpdated         DATETIME2 NULL,
        DeletedBy           NVARCHAR(128) NULL,
        DateDeleted         DATETIME2 NULL,
        IsDeleted           BIT NOT NULL CONSTRAINT DF_tblScreenImageSlot_IsDeleted DEFAULT (0),

        CONSTRAINT PK_tblScreenImageSlot_ScreenImageSlotId PRIMARY KEY CLUSTERED (ScreenImageSlotId)
    );

    CREATE UNIQUE INDEX UK_tblScreenImageSlot_PageKey_SlotKey_Breakpoint
        ON dbo.tblScreenImageSlot (PageKey, SlotKey, Breakpoint);
END
GO
