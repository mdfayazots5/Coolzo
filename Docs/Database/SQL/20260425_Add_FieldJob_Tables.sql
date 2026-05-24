-- 2026-04-25
-- Align field workflow persistence tables with Backend/Coolzo.Domain/Entities/FieldWorkflowEntities.cs
-- and Backend/Coolzo.Persistence/Configurations/{FieldWorkflowConfiguration.cs,GapPhaseAEntityConfigurations.cs}
-- Tables: tblJobReport, tblJobPhoto, tblCustomerSignature, tblOfflineSyncQueueItem

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tblJobReport') AND type = N'U')
BEGIN
    CREATE TABLE dbo.tblJobReport
    (
        JobReportId         BIGINT IDENTITY(1,1) NOT NULL,
        CompanyId           INT NOT NULL CONSTRAINT DF_tblJobReport_CompanyId DEFAULT (1),
        SiteId              INT NOT NULL CONSTRAINT DF_tblJobReport_SiteId DEFAULT (1),
        BranchId            INT NOT NULL CONSTRAINT DF_tblJobReport_BranchId DEFAULT (1),
        DepartmentId        INT NULL,

        ServiceRequestId    BIGINT NOT NULL,
        JobCardId           BIGINT NOT NULL,
        TechnicianId        BIGINT NOT NULL,
        IdempotencyKey      NVARCHAR(128) NOT NULL CONSTRAINT DF_tblJobReport_IdempotencyKey DEFAULT (N''),
        EquipmentCondition  NVARCHAR(64) NOT NULL CONSTRAINT DF_tblJobReport_EquipmentCondition DEFAULT (N''),
        IssuesIdentifiedJson NVARCHAR(MAX) NOT NULL CONSTRAINT DF_tblJobReport_IssuesIdentifiedJson DEFAULT (N'[]'),
        ActionTaken         NVARCHAR(MAX) NOT NULL CONSTRAINT DF_tblJobReport_ActionTaken DEFAULT (N''),
        Recommendation      NVARCHAR(MAX) NOT NULL CONSTRAINT DF_tblJobReport_Recommendation DEFAULT (N''),
        Observations        NVARCHAR(MAX) NOT NULL CONSTRAINT DF_tblJobReport_Observations DEFAULT (N''),
        SubmittedAtUtc      DATETIME2 NOT NULL CONSTRAINT DF_tblJobReport_SubmittedAtUtc DEFAULT (GETUTCDATE()),
        IsQualityReviewed   BIT NOT NULL CONSTRAINT DF_tblJobReport_IsQualityReviewed DEFAULT (0),
        QualityScore        DECIMAL(5,2) NOT NULL CONSTRAINT DF_tblJobReport_QualityScore DEFAULT (0),

        Tag                 NVARCHAR(64) NULL,
        Comments            NVARCHAR(512) NULL,
        DisplayOnWeb        BIT NOT NULL CONSTRAINT DF_tblJobReport_DisplayOnWeb DEFAULT (1),
        IsPublished         BIT NOT NULL CONSTRAINT DF_tblJobReport_IsPublished DEFAULT (1),
        DatePublished       DATETIME2 NULL,
        PublishedBy         NVARCHAR(128) NULL,
        SortOrder           INT NOT NULL CONSTRAINT DF_tblJobReport_SortOrder DEFAULT (0),
        IPAddress           NVARCHAR(64) NOT NULL CONSTRAINT DF_tblJobReport_IPAddress DEFAULT (N'127.0.0.1'),
        CreatedBy           NVARCHAR(128) NOT NULL CONSTRAINT DF_tblJobReport_CreatedBy DEFAULT (N'System'),
        DateCreated         DATETIME2 NOT NULL CONSTRAINT DF_tblJobReport_DateCreated DEFAULT (GETUTCDATE()),
        UpdatedBy           NVARCHAR(128) NULL,
        LastUpdated         DATETIME2 NULL,
        DeletedBy           NVARCHAR(128) NULL,
        DateDeleted         DATETIME2 NULL,
        IsDeleted           BIT NOT NULL CONSTRAINT DF_tblJobReport_IsDeleted DEFAULT (0),

        CONSTRAINT PK_tblJobReport PRIMARY KEY (JobReportId),
        CONSTRAINT FK_tblJobReport_ServiceRequestId_tblServiceRequest_ServiceRequestId
            FOREIGN KEY (ServiceRequestId) REFERENCES dbo.tblServiceRequest(ServiceRequestId),
        CONSTRAINT FK_tblJobReport_JobCardId_tblJobCard_JobCardId
            FOREIGN KEY (JobCardId) REFERENCES dbo.tblJobCard(JobCardId),
        CONSTRAINT FK_tblJobReport_TechnicianId_tblTechnician_TechnicianId
            FOREIGN KEY (TechnicianId) REFERENCES dbo.tblTechnician(TechnicianId)
    );

    CREATE INDEX IDX_tblJobReport_ServiceRequestId_SubmittedAtUtc
        ON dbo.tblJobReport(ServiceRequestId, SubmittedAtUtc);

    CREATE INDEX IDX_tblJobReport_IdempotencyKey
        ON dbo.tblJobReport(IdempotencyKey);
END

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tblJobPhoto') AND type = N'U')
BEGIN
    CREATE TABLE dbo.tblJobPhoto
    (
        JobPhotoId          BIGINT IDENTITY(1,1) NOT NULL,
        CompanyId           INT NOT NULL CONSTRAINT DF_tblJobPhoto_CompanyId DEFAULT (1),
        SiteId              INT NOT NULL CONSTRAINT DF_tblJobPhoto_SiteId DEFAULT (1),
        BranchId            INT NOT NULL CONSTRAINT DF_tblJobPhoto_BranchId DEFAULT (1),
        DepartmentId        INT NULL,

        ServiceRequestId    BIGINT NOT NULL,
        JobCardId           BIGINT NOT NULL,
        TechnicianId        BIGINT NOT NULL,
        JobReportId         BIGINT NULL,
        PhotoType           INT NOT NULL CONSTRAINT DF_tblJobPhoto_PhotoType DEFAULT (0),
        FileName            NVARCHAR(256) NOT NULL,
        ContentType         NVARCHAR(128) NOT NULL,
        StorageUrl          NVARCHAR(512) NOT NULL CONSTRAINT DF_tblJobPhoto_StorageUrl DEFAULT (N''),
        UploadedBy          NVARCHAR(128) NOT NULL CONSTRAINT DF_tblJobPhoto_UploadedBy DEFAULT (N'System'),
        UploadedAtUtc       DATETIME2 NOT NULL CONSTRAINT DF_tblJobPhoto_UploadedAtUtc DEFAULT (GETUTCDATE()),
        PhotoRemarks        NVARCHAR(512) NOT NULL CONSTRAINT DF_tblJobPhoto_PhotoRemarks DEFAULT (N''),

        Tag                 NVARCHAR(64) NULL,
        Comments            NVARCHAR(512) NULL,
        DisplayOnWeb        BIT NOT NULL CONSTRAINT DF_tblJobPhoto_DisplayOnWeb DEFAULT (1),
        IsPublished         BIT NOT NULL CONSTRAINT DF_tblJobPhoto_IsPublished DEFAULT (1),
        DatePublished       DATETIME2 NULL,
        PublishedBy         NVARCHAR(128) NULL,
        SortOrder           INT NOT NULL CONSTRAINT DF_tblJobPhoto_SortOrder DEFAULT (0),
        IPAddress           NVARCHAR(64) NOT NULL CONSTRAINT DF_tblJobPhoto_IPAddress DEFAULT (N'127.0.0.1'),
        CreatedBy           NVARCHAR(128) NOT NULL CONSTRAINT DF_tblJobPhoto_CreatedBy DEFAULT (N'System'),
        DateCreated         DATETIME2 NOT NULL CONSTRAINT DF_tblJobPhoto_DateCreated DEFAULT (GETUTCDATE()),
        UpdatedBy           NVARCHAR(128) NULL,
        LastUpdated         DATETIME2 NULL,
        DeletedBy           NVARCHAR(128) NULL,
        DateDeleted         DATETIME2 NULL,
        IsDeleted           BIT NOT NULL CONSTRAINT DF_tblJobPhoto_IsDeleted DEFAULT (0),

        CONSTRAINT PK_tblJobPhoto PRIMARY KEY (JobPhotoId),
        CONSTRAINT FK_tblJobPhoto_ServiceRequestId_tblServiceRequest_ServiceRequestId
            FOREIGN KEY (ServiceRequestId) REFERENCES dbo.tblServiceRequest(ServiceRequestId),
        CONSTRAINT FK_tblJobPhoto_JobCardId_tblJobCard_JobCardId
            FOREIGN KEY (JobCardId) REFERENCES dbo.tblJobCard(JobCardId),
        CONSTRAINT FK_tblJobPhoto_TechnicianId_tblTechnician_TechnicianId
            FOREIGN KEY (TechnicianId) REFERENCES dbo.tblTechnician(TechnicianId),
        CONSTRAINT FK_tblJobPhoto_JobReportId_tblJobReport_JobReportId
            FOREIGN KEY (JobReportId) REFERENCES dbo.tblJobReport(JobReportId)
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tblCustomerSignature') AND type = N'U')
BEGIN
    CREATE TABLE dbo.tblCustomerSignature
    (
        CustomerSignatureId BIGINT IDENTITY(1,1) NOT NULL,
        CompanyId           INT NOT NULL CONSTRAINT DF_tblCustomerSignature_CompanyId DEFAULT (1),
        SiteId              INT NOT NULL CONSTRAINT DF_tblCustomerSignature_SiteId DEFAULT (1),
        BranchId            INT NOT NULL CONSTRAINT DF_tblCustomerSignature_BranchId DEFAULT (1),
        DepartmentId        INT NULL,

        ServiceRequestId    BIGINT NOT NULL,
        JobCardId           BIGINT NOT NULL,
        TechnicianId        BIGINT NOT NULL,
        JobReportId         BIGINT NULL,
        CustomerName        NVARCHAR(128) NOT NULL CONSTRAINT DF_tblCustomerSignature_CustomerName DEFAULT (N''),
        SignatureDataUrl    NVARCHAR(MAX) NOT NULL CONSTRAINT DF_tblCustomerSignature_SignatureDataUrl DEFAULT (N''),
        SignedAtUtc         DATETIME2 NOT NULL CONSTRAINT DF_tblCustomerSignature_SignedAtUtc DEFAULT (GETUTCDATE()),
        CapturedBy          NVARCHAR(128) NOT NULL CONSTRAINT DF_tblCustomerSignature_CapturedBy DEFAULT (N'System'),
        SignatureRemarks    NVARCHAR(512) NOT NULL CONSTRAINT DF_tblCustomerSignature_SignatureRemarks DEFAULT (N''),

        Tag                 NVARCHAR(64) NULL,
        Comments            NVARCHAR(512) NULL,
        DisplayOnWeb        BIT NOT NULL CONSTRAINT DF_tblCustomerSignature_DisplayOnWeb DEFAULT (1),
        IsPublished         BIT NOT NULL CONSTRAINT DF_tblCustomerSignature_IsPublished DEFAULT (1),
        DatePublished       DATETIME2 NULL,
        PublishedBy         NVARCHAR(128) NULL,
        SortOrder           INT NOT NULL CONSTRAINT DF_tblCustomerSignature_SortOrder DEFAULT (0),
        IPAddress           NVARCHAR(64) NOT NULL CONSTRAINT DF_tblCustomerSignature_IPAddress DEFAULT (N'127.0.0.1'),
        CreatedBy           NVARCHAR(128) NOT NULL CONSTRAINT DF_tblCustomerSignature_CreatedBy DEFAULT (N'System'),
        DateCreated         DATETIME2 NOT NULL CONSTRAINT DF_tblCustomerSignature_DateCreated DEFAULT (GETUTCDATE()),
        UpdatedBy           NVARCHAR(128) NULL,
        LastUpdated         DATETIME2 NULL,
        DeletedBy           NVARCHAR(128) NULL,
        DateDeleted         DATETIME2 NULL,
        IsDeleted           BIT NOT NULL CONSTRAINT DF_tblCustomerSignature_IsDeleted DEFAULT (0),

        CONSTRAINT PK_tblCustomerSignature PRIMARY KEY (CustomerSignatureId),
        CONSTRAINT FK_tblCustomerSignature_ServiceRequestId_tblServiceRequest_ServiceRequestId
            FOREIGN KEY (ServiceRequestId) REFERENCES dbo.tblServiceRequest(ServiceRequestId),
        CONSTRAINT FK_tblCustomerSignature_JobCardId_tblJobCard_JobCardId
            FOREIGN KEY (JobCardId) REFERENCES dbo.tblJobCard(JobCardId),
        CONSTRAINT FK_tblCustomerSignature_TechnicianId_tblTechnician_TechnicianId
            FOREIGN KEY (TechnicianId) REFERENCES dbo.tblTechnician(TechnicianId),
        CONSTRAINT FK_tblCustomerSignature_JobReportId_tblJobReport_JobReportId
            FOREIGN KEY (JobReportId) REFERENCES dbo.tblJobReport(JobReportId)
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tblOfflineSyncQueueItem') AND type = N'U')
BEGIN
    CREATE TABLE dbo.tblOfflineSyncQueueItem
    (
        OfflineSyncQueueItemId BIGINT IDENTITY(1,1) NOT NULL,
        CompanyId              INT NOT NULL CONSTRAINT DF_tblOfflineSyncQueueItem_CompanyId DEFAULT (1),
        SiteId                 INT NOT NULL CONSTRAINT DF_tblOfflineSyncQueueItem_SiteId DEFAULT (1),
        BranchId               INT NOT NULL CONSTRAINT DF_tblOfflineSyncQueueItem_BranchId DEFAULT (1),
        DepartmentId           INT NULL,

        DeviceReference        NVARCHAR(128) NOT NULL,
        EntityName             NVARCHAR(64) NOT NULL,
        EntityReference        NVARCHAR(128) NOT NULL,
        PayloadSnapshot        NVARCHAR(4000) NOT NULL CONSTRAINT DF_tblOfflineSyncQueueItem_PayloadSnapshot DEFAULT (N''),
        SyncStatus             INT NOT NULL CONSTRAINT DF_tblOfflineSyncQueueItem_SyncStatus DEFAULT (0),
        RetryCount             INT NOT NULL CONSTRAINT DF_tblOfflineSyncQueueItem_RetryCount DEFAULT (0),
        LastAttemptDateUtc     DATETIME2 NOT NULL CONSTRAINT DF_tblOfflineSyncQueueItem_LastAttemptDateUtc DEFAULT (GETUTCDATE()),
        NextRetryDateUtc       DATETIME2 NULL,
        ConflictStrategy       NVARCHAR(128) NOT NULL CONSTRAINT DF_tblOfflineSyncQueueItem_ConflictStrategy DEFAULT (N''),
        FailureReason          NVARCHAR(512) NOT NULL CONSTRAINT DF_tblOfflineSyncQueueItem_FailureReason DEFAULT (N''),

        Tag                    NVARCHAR(64) NULL,
        Comments               NVARCHAR(512) NULL,
        DisplayOnWeb           BIT NOT NULL CONSTRAINT DF_tblOfflineSyncQueueItem_DisplayOnWeb DEFAULT (1),
        IsPublished            BIT NOT NULL CONSTRAINT DF_tblOfflineSyncQueueItem_IsPublished DEFAULT (1),
        DatePublished          DATETIME2 NULL,
        PublishedBy            NVARCHAR(128) NULL,
        SortOrder              INT NOT NULL CONSTRAINT DF_tblOfflineSyncQueueItem_SortOrder DEFAULT (0),
        IPAddress              NVARCHAR(64) NOT NULL CONSTRAINT DF_tblOfflineSyncQueueItem_IPAddress DEFAULT (N'127.0.0.1'),
        CreatedBy              NVARCHAR(128) NOT NULL CONSTRAINT DF_tblOfflineSyncQueueItem_CreatedBy DEFAULT (N'System'),
        DateCreated            DATETIME2 NOT NULL CONSTRAINT DF_tblOfflineSyncQueueItem_DateCreated DEFAULT (GETUTCDATE()),
        UpdatedBy              NVARCHAR(128) NULL,
        LastUpdated            DATETIME2 NULL,
        DeletedBy              NVARCHAR(128) NULL,
        DateDeleted            DATETIME2 NULL,
        IsDeleted              BIT NOT NULL CONSTRAINT DF_tblOfflineSyncQueueItem_IsDeleted DEFAULT (0),

        CONSTRAINT PK_tblOfflineSyncQueueItem_OfflineSyncQueueItemId PRIMARY KEY (OfflineSyncQueueItemId)
    );

    CREATE INDEX IDX_tblOfflineSyncQueueItem_EntityName_EntityReference
        ON dbo.tblOfflineSyncQueueItem(EntityName, EntityReference);

    CREATE INDEX IDX_tblOfflineSyncQueueItem_SyncStatus_NextRetryDateUtc
        ON dbo.tblOfflineSyncQueueItem(SyncStatus, NextRetryDateUtc);
END

-- End of migration: 2026-04-25
