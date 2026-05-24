-- 2026-04-25
-- Add inventory parts-request persistence tables aligned with:
-- Backend/Coolzo.Domain/Entities/FieldWorkflowEntities.cs
-- Backend/Coolzo.Persistence/Configurations/FieldWorkflowConfiguration.cs
-- Tables: tblPartsRequest, tblPartsRequestItem

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tblPartsRequest') AND type = N'U')
BEGIN
    CREATE TABLE dbo.tblPartsRequest
    (
        PartsRequestId       BIGINT IDENTITY(1,1) NOT NULL,
        CompanyId            INT NOT NULL CONSTRAINT DF_tblPartsRequest_CompanyId DEFAULT (1),
        SiteId               INT NOT NULL CONSTRAINT DF_tblPartsRequest_SiteId DEFAULT (1),
        BranchId             INT NOT NULL CONSTRAINT DF_tblPartsRequest_BranchId DEFAULT (1),
        DepartmentId         INT NULL,

        ServiceRequestId     BIGINT NOT NULL,
        JobCardId            BIGINT NOT NULL,
        TechnicianId         BIGINT NOT NULL,
        Urgency              INT NOT NULL CONSTRAINT DF_tblPartsRequest_Urgency DEFAULT (1),
        CurrentStatus        INT NOT NULL CONSTRAINT DF_tblPartsRequest_CurrentStatus DEFAULT (1),
        Notes                NVARCHAR(1024) NOT NULL CONSTRAINT DF_tblPartsRequest_Notes DEFAULT (N''),
        SubmittedAtUtc       DATETIME2 NOT NULL CONSTRAINT DF_tblPartsRequest_SubmittedAtUtc DEFAULT (GETUTCDATE()),
        ProcessedAtUtc       DATETIME2 NULL,

        Tag                  NVARCHAR(64) NULL,
        Comments             NVARCHAR(512) NULL,
        DisplayOnWeb         BIT NOT NULL CONSTRAINT DF_tblPartsRequest_DisplayOnWeb DEFAULT (1),
        IsPublished          BIT NOT NULL CONSTRAINT DF_tblPartsRequest_IsPublished DEFAULT (1),
        DatePublished        DATETIME2 NULL,
        PublishedBy          NVARCHAR(128) NULL,
        SortOrder            INT NOT NULL CONSTRAINT DF_tblPartsRequest_SortOrder DEFAULT (0),
        IPAddress            NVARCHAR(64) NOT NULL CONSTRAINT DF_tblPartsRequest_IPAddress DEFAULT (N'127.0.0.1'),
        CreatedBy            NVARCHAR(128) NOT NULL CONSTRAINT DF_tblPartsRequest_CreatedBy DEFAULT (N'System'),
        DateCreated          DATETIME2 NOT NULL CONSTRAINT DF_tblPartsRequest_DateCreated DEFAULT (GETUTCDATE()),
        UpdatedBy            NVARCHAR(128) NULL,
        LastUpdated          DATETIME2 NULL,
        DeletedBy            NVARCHAR(128) NULL,
        DateDeleted          DATETIME2 NULL,
        IsDeleted            BIT NOT NULL CONSTRAINT DF_tblPartsRequest_IsDeleted DEFAULT (0),

        CONSTRAINT PK_tblPartsRequest PRIMARY KEY (PartsRequestId),
        CONSTRAINT FK_tblPartsRequest_ServiceRequestId_tblServiceRequest_ServiceRequestId
            FOREIGN KEY (ServiceRequestId) REFERENCES dbo.tblServiceRequest(ServiceRequestId),
        CONSTRAINT FK_tblPartsRequest_JobCardId_tblJobCard_JobCardId
            FOREIGN KEY (JobCardId) REFERENCES dbo.tblJobCard(JobCardId),
        CONSTRAINT FK_tblPartsRequest_TechnicianId_tblTechnician_TechnicianId
            FOREIGN KEY (TechnicianId) REFERENCES dbo.tblTechnician(TechnicianId)
    );

    CREATE INDEX IDX_tblPartsRequest_ServiceRequestId_SubmittedAtUtc
        ON dbo.tblPartsRequest(ServiceRequestId, SubmittedAtUtc DESC);

    CREATE INDEX IDX_tblPartsRequest_CurrentStatus_Urgency
        ON dbo.tblPartsRequest(CurrentStatus, Urgency);
END

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tblPartsRequestItem') AND type = N'U')
BEGIN
    CREATE TABLE dbo.tblPartsRequestItem
    (
        PartsRequestItemId   BIGINT IDENTITY(1,1) NOT NULL,
        CompanyId            INT NOT NULL CONSTRAINT DF_tblPartsRequestItem_CompanyId DEFAULT (1),
        SiteId               INT NOT NULL CONSTRAINT DF_tblPartsRequestItem_SiteId DEFAULT (1),
        BranchId             INT NOT NULL CONSTRAINT DF_tblPartsRequestItem_BranchId DEFAULT (1),
        DepartmentId         INT NULL,

        PartsRequestId       BIGINT NOT NULL,
        ItemId               BIGINT NULL,
        PartCode             NVARCHAR(64) NOT NULL,
        PartName             NVARCHAR(256) NOT NULL,
        QuantityRequested    DECIMAL(12,2) NOT NULL CONSTRAINT DF_tblPartsRequestItem_QuantityRequested DEFAULT (0),
        QuantityApproved     DECIMAL(12,2) NOT NULL CONSTRAINT DF_tblPartsRequestItem_QuantityApproved DEFAULT (0),
        ItemRemarks          NVARCHAR(512) NOT NULL CONSTRAINT DF_tblPartsRequestItem_ItemRemarks DEFAULT (N''),
        CurrentStatus        INT NOT NULL CONSTRAINT DF_tblPartsRequestItem_CurrentStatus DEFAULT (1),

        Tag                  NVARCHAR(64) NULL,
        Comments             NVARCHAR(512) NULL,
        DisplayOnWeb         BIT NOT NULL CONSTRAINT DF_tblPartsRequestItem_DisplayOnWeb DEFAULT (1),
        IsPublished          BIT NOT NULL CONSTRAINT DF_tblPartsRequestItem_IsPublished DEFAULT (1),
        DatePublished        DATETIME2 NULL,
        PublishedBy          NVARCHAR(128) NULL,
        SortOrder            INT NOT NULL CONSTRAINT DF_tblPartsRequestItem_SortOrder DEFAULT (0),
        IPAddress            NVARCHAR(64) NOT NULL CONSTRAINT DF_tblPartsRequestItem_IPAddress DEFAULT (N'127.0.0.1'),
        CreatedBy            NVARCHAR(128) NOT NULL CONSTRAINT DF_tblPartsRequestItem_CreatedBy DEFAULT (N'System'),
        DateCreated          DATETIME2 NOT NULL CONSTRAINT DF_tblPartsRequestItem_DateCreated DEFAULT (GETUTCDATE()),
        UpdatedBy            NVARCHAR(128) NULL,
        LastUpdated          DATETIME2 NULL,
        DeletedBy            NVARCHAR(128) NULL,
        DateDeleted          DATETIME2 NULL,
        IsDeleted            BIT NOT NULL CONSTRAINT DF_tblPartsRequestItem_IsDeleted DEFAULT (0),

        CONSTRAINT PK_tblPartsRequestItem PRIMARY KEY (PartsRequestItemId),
        CONSTRAINT FK_tblPartsRequestItem_PartsRequestId_tblPartsRequest_PartsRequestId
            FOREIGN KEY (PartsRequestId) REFERENCES dbo.tblPartsRequest(PartsRequestId),
        CONSTRAINT FK_tblPartsRequestItem_ItemId_tblItem_ItemId
            FOREIGN KEY (ItemId) REFERENCES dbo.tblItem(ItemId)
    );

    CREATE INDEX IDX_tblPartsRequestItem_PartsRequestId
        ON dbo.tblPartsRequestItem(PartsRequestId);

    CREATE INDEX IDX_tblPartsRequestItem_ItemId
        ON dbo.tblPartsRequestItem(ItemId);
END

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tblPartsRequest') AND type = N'U')
   AND NOT EXISTS (SELECT 1 FROM dbo.tblPartsRequest)
BEGIN
    DECLARE @SeedRequests TABLE
    (
        SeedRowNo            INT NOT NULL PRIMARY KEY,
        PartsRequestId       BIGINT NOT NULL,
        CompanyId            INT NOT NULL,
        SiteId               INT NOT NULL,
        BranchId             INT NOT NULL,
        DepartmentId         INT NULL
    );

    ;WITH SeedSource AS
    (
        SELECT TOP (2)
            ROW_NUMBER() OVER (ORDER BY sr.ServiceRequestId) AS SeedRowNo,
            sr.CompanyId,
            sr.SiteId,
            sr.BranchId,
            sr.DepartmentId,
            sr.ServiceRequestId,
            jc.JobCardId,
            tech.TechnicianId
        FROM dbo.tblServiceRequest sr
        INNER JOIN dbo.tblJobCard jc
            ON jc.ServiceRequestId = sr.ServiceRequestId
           AND jc.IsDeleted = 0
        CROSS JOIN
        (
            SELECT TOP (1) TechnicianId
            FROM dbo.tblTechnician
            WHERE IsDeleted = 0
            ORDER BY TechnicianId
        ) tech
        WHERE sr.IsDeleted = 0
        ORDER BY sr.ServiceRequestId
    )
    INSERT INTO dbo.tblPartsRequest
    (
        CompanyId,
        SiteId,
        BranchId,
        DepartmentId,
        ServiceRequestId,
        JobCardId,
        TechnicianId,
        Urgency,
        CurrentStatus,
        Notes,
        SubmittedAtUtc,
        ProcessedAtUtc,
        Tag,
        Comments,
        DisplayOnWeb,
        IsPublished,
        DatePublished,
        PublishedBy,
        SortOrder,
        IPAddress,
        CreatedBy,
        DateCreated,
        IsDeleted
    )
    SELECT
        src.CompanyId,
        src.SiteId,
        src.BranchId,
        src.DepartmentId,
        src.ServiceRequestId,
        src.JobCardId,
        src.TechnicianId,
        CASE WHEN src.SeedRowNo = 1 THEN 2 ELSE 1 END,
        CASE WHEN src.SeedRowNo = 1 THEN 1 ELSE 3 END,
        CASE
            WHEN src.SeedRowNo = 1 THEN N'Urgent compressor restart request for same-day completion.'
            ELSE N'Partial issue created while waiting for the remaining electrical spare.'
        END,
        DATEADD(MINUTE, -(src.SeedRowNo * 45), GETUTCDATE()),
        CASE WHEN src.SeedRowNo = 1 THEN NULL ELSE DATEADD(MINUTE, -15, GETUTCDATE()) END,
        N'LIVE-SEED',
        N'Inventory parts-request seed created to unblock AdminMobile queue.',
        1,
        1,
        GETUTCDATE(),
        N'Seed20260425',
        src.SeedRowNo,
        N'127.0.0.1',
        N'Seed20260425',
        GETUTCDATE(),
        0
    FROM SeedSource src;

    INSERT INTO @SeedRequests
    (
        SeedRowNo,
        PartsRequestId,
        CompanyId,
        SiteId,
        BranchId,
        DepartmentId
    )
    SELECT
        req.SortOrder,
        req.PartsRequestId,
        req.CompanyId,
        req.SiteId,
        req.BranchId,
        req.DepartmentId
    FROM dbo.tblPartsRequest req
    WHERE req.Tag = N'LIVE-SEED'
      AND req.CreatedBy = N'Seed20260425';

    INSERT INTO dbo.tblPartsRequestItem
    (
        CompanyId,
        SiteId,
        BranchId,
        DepartmentId,
        PartsRequestId,
        ItemId,
        PartCode,
        PartName,
        QuantityRequested,
        QuantityApproved,
        ItemRemarks,
        CurrentStatus,
        Tag,
        Comments,
        DisplayOnWeb,
        IsPublished,
        DatePublished,
        PublishedBy,
        SortOrder,
        IPAddress,
        CreatedBy,
        DateCreated,
        IsDeleted
    )
    SELECT
        seed.CompanyId,
        seed.SiteId,
        seed.BranchId,
        seed.DepartmentId,
        seed.PartsRequestId,
        inventoryItem.ItemId,
        item.PartCode,
        item.PartName,
        item.QuantityRequested,
        item.QuantityApproved,
        item.ItemRemarks,
        item.CurrentStatus,
        N'LIVE-SEED',
        N'Inventory parts-request item seed created to unblock AdminMobile queue.',
        1,
        1,
        GETUTCDATE(),
        N'Seed20260425',
        item.SortOrder,
        N'127.0.0.1',
        N'Seed20260425',
        GETUTCDATE(),
        0
    FROM @SeedRequests seed
    INNER JOIN
    (
        SELECT
            1 AS SeedRowNo,
            N'CAP-45' AS PartCode,
            N'Capacitor 45mfd' AS PartName,
            CAST(1.00 AS DECIMAL(12,2)) AS QuantityRequested,
            CAST(0.00 AS DECIMAL(12,2)) AS QuantityApproved,
            N'Critical for compressor restart.' AS ItemRemarks,
            1 AS CurrentStatus,
            1 AS SortOrder
        UNION ALL
        SELECT
            1,
            N'CONT-2P',
            N'2 Pole Contactor',
            CAST(1.00 AS DECIMAL(12,2)),
            CAST(0.00 AS DECIMAL(12,2)),
            N'Hold with capacitor for same visit completion.',
            1,
            2
        UNION ALL
        SELECT
            2,
            N'FUSE-32A',
            N'32A Cartridge Fuse',
            CAST(2.00 AS DECIMAL(12,2)),
            CAST(2.00 AS DECIMAL(12,2)),
            N'Approved and issued from branch stock.',
            2,
            1
        UNION ALL
        SELECT
            2,
            N'PCB-IDU',
            N'Indoor Unit Control PCB',
            CAST(1.00 AS DECIMAL(12,2)),
            CAST(0.00 AS DECIMAL(12,2)),
            N'Pending supplier replenishment.',
            3,
            2
    ) item
        ON item.SeedRowNo = seed.SeedRowNo
    LEFT JOIN dbo.tblItem inventoryItem
        ON inventoryItem.ItemCode = item.PartCode
       AND inventoryItem.IsDeleted = 0;
END

-- End of migration: 2026-04-25
