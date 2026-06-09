-- 2026-06-09
-- CMS Content & Theme Delivery (Phase 1) — published content snapshot version registry.
-- Aligns with Backend/Coolzo.Domain/Entities/PublishedSnapshot.cs and
-- Backend/Coolzo.Persistence/Configurations/PublishedSnapshotConfiguration.cs
-- Table: tblPublishedSnapshot

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tblPublishedSnapshot') AND type = N'U')
BEGIN
    CREATE TABLE dbo.tblPublishedSnapshot
    (
        PublishedSnapshotId BIGINT IDENTITY(1,1) NOT NULL,
        CompanyId           INT NOT NULL CONSTRAINT DF_tblPublishedSnapshot_CompanyId DEFAULT (1),
        SiteId              INT NOT NULL CONSTRAINT DF_tblPublishedSnapshot_SiteId DEFAULT (1),
        BranchId            INT NOT NULL CONSTRAINT DF_tblPublishedSnapshot_BranchId DEFAULT (1),
        DepartmentId        INT NULL,

        [Version]           INT NOT NULL,
        BucketKey           NVARCHAR(256) NOT NULL CONSTRAINT DF_tblPublishedSnapshot_BucketKey DEFAULT (N''),
        BucketUrl           NVARCHAR(512) NOT NULL CONSTRAINT DF_tblPublishedSnapshot_BucketUrl DEFAULT (N''),
        ChecksumHash        NVARCHAR(128) NOT NULL CONSTRAINT DF_tblPublishedSnapshot_ChecksumHash DEFAULT (N''),
        PayloadSizeBytes    BIGINT NOT NULL CONSTRAINT DF_tblPublishedSnapshot_PayloadSizeBytes DEFAULT (0),
        IsActive            BIT NOT NULL CONSTRAINT DF_tblPublishedSnapshot_IsActive DEFAULT (0),

        Tag                 NVARCHAR(64) NULL,
        Comments            NVARCHAR(512) NULL,
        DisplayOnWeb        BIT NOT NULL CONSTRAINT DF_tblPublishedSnapshot_DisplayOnWeb DEFAULT (1),
        IsPublished         BIT NOT NULL CONSTRAINT DF_tblPublishedSnapshot_IsPublished DEFAULT (1),
        DatePublished       DATETIME2 NULL,
        PublishedBy         NVARCHAR(128) NULL,
        SortOrder           INT NOT NULL CONSTRAINT DF_tblPublishedSnapshot_SortOrder DEFAULT (0),
        IPAddress           NVARCHAR(64) NOT NULL CONSTRAINT DF_tblPublishedSnapshot_IPAddress DEFAULT (N'127.0.0.1'),
        CreatedBy           NVARCHAR(128) NOT NULL CONSTRAINT DF_tblPublishedSnapshot_CreatedBy DEFAULT (N'System'),
        DateCreated         DATETIME2 NOT NULL CONSTRAINT DF_tblPublishedSnapshot_DateCreated DEFAULT (GETUTCDATE()),
        UpdatedBy           NVARCHAR(128) NULL,
        LastUpdated         DATETIME2 NULL,
        DeletedBy           NVARCHAR(128) NULL,
        DateDeleted         DATETIME2 NULL,
        IsDeleted           BIT NOT NULL CONSTRAINT DF_tblPublishedSnapshot_IsDeleted DEFAULT (0),

        CONSTRAINT PK_tblPublishedSnapshot_PublishedSnapshotId PRIMARY KEY CLUSTERED (PublishedSnapshotId)
    );

    CREATE UNIQUE INDEX UK_tblPublishedSnapshot_Version
        ON dbo.tblPublishedSnapshot ([Version]);

    CREATE INDEX IDX_tblPublishedSnapshot_IsActive
        ON dbo.tblPublishedSnapshot (IsActive);
END
GO
