IF NOT EXISTS (
    SELECT 1
    FROM sys.objects
    WHERE object_id = OBJECT_ID(N'dbo.tblCustomerEquipment')
      AND type = N'U'
)
BEGIN
    CREATE TABLE dbo.tblCustomerEquipment
    (
        CustomerEquipmentId BIGINT IDENTITY(1,1) NOT NULL,
        CustomerId BIGINT NOT NULL,
        EquipmentName NVARCHAR(128) NOT NULL CONSTRAINT DF_tblCustomerEquipment_EquipmentName DEFAULT (N''),
        EquipmentType NVARCHAR(64) NOT NULL CONSTRAINT DF_tblCustomerEquipment_EquipmentType DEFAULT (N''),
        BrandName NVARCHAR(128) NOT NULL CONSTRAINT DF_tblCustomerEquipment_BrandName DEFAULT (N''),
        Capacity NVARCHAR(64) NOT NULL CONSTRAINT DF_tblCustomerEquipment_Capacity DEFAULT (N''),
        LocationLabel NVARCHAR(128) NOT NULL CONSTRAINT DF_tblCustomerEquipment_LocationLabel DEFAULT (N''),
        PurchaseDate DATE NULL,
        LastServiceDate DATE NULL,
        SerialNumber NVARCHAR(128) NOT NULL CONSTRAINT DF_tblCustomerEquipment_SerialNumber DEFAULT (N''),
        IsActive BIT NOT NULL CONSTRAINT DF_tblCustomerEquipment_IsActive DEFAULT (1),
        CompanyId INT NOT NULL CONSTRAINT DF_tblCustomerEquipment_CompanyId DEFAULT (1),
        SiteId INT NOT NULL CONSTRAINT DF_tblCustomerEquipment_SiteId DEFAULT (1),
        BranchId INT NOT NULL CONSTRAINT DF_tblCustomerEquipment_BranchId DEFAULT (1),
        DepartmentId INT NULL,
        Tag NVARCHAR(64) NULL,
        Comments NVARCHAR(512) NULL,
        DisplayOnWeb BIT NOT NULL CONSTRAINT DF_tblCustomerEquipment_DisplayOnWeb DEFAULT (1),
        IsPublished BIT NOT NULL CONSTRAINT DF_tblCustomerEquipment_IsPublished DEFAULT (1),
        DatePublished DATETIME2 NULL,
        PublishedBy NVARCHAR(128) NULL,
        SortOrder INT NOT NULL CONSTRAINT DF_tblCustomerEquipment_SortOrder DEFAULT (0),
        IPAddress NVARCHAR(64) NOT NULL CONSTRAINT DF_tblCustomerEquipment_IPAddress DEFAULT (N'127.0.0.1'),
        CreatedBy NVARCHAR(128) NOT NULL CONSTRAINT DF_tblCustomerEquipment_CreatedBy DEFAULT (N'System'),
        DateCreated DATETIME2 NOT NULL CONSTRAINT DF_tblCustomerEquipment_DateCreated DEFAULT (GETUTCDATE()),
        UpdatedBy NVARCHAR(128) NULL,
        LastUpdated DATETIME2 NULL,
        DeletedBy NVARCHAR(128) NULL,
        DateDeleted DATETIME2 NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_tblCustomerEquipment_IsDeleted DEFAULT (0),
        CONSTRAINT PK_tblCustomerEquipment_CustomerEquipmentId PRIMARY KEY (CustomerEquipmentId),
        CONSTRAINT FK_tblCustomerEquipment_CustomerId_tblCustomer_CustomerId
            FOREIGN KEY (CustomerId) REFERENCES dbo.tblCustomer (CustomerId)
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IDX_tblCustomerEquipment_CustomerId_IsActive'
      AND object_id = OBJECT_ID(N'dbo.tblCustomerEquipment')
)
BEGIN
    CREATE INDEX IDX_tblCustomerEquipment_CustomerId_IsActive
        ON dbo.tblCustomerEquipment (CustomerId, IsActive);
END;
