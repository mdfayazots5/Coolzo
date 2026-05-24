-- 2026-05-06
-- Add inventory purchase-order persistence tables aligned with:
-- Backend/Coolzo.Domain/Entities/PurchaseOrder.cs
-- Backend/Coolzo.Persistence/Configurations/PurchaseOrderConfiguration.cs
-- Tables: tblPurchaseOrder, tblPurchaseOrderItem

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tblPurchaseOrder') AND type = N'U')
BEGIN
    CREATE TABLE dbo.tblPurchaseOrder
    (
        PurchaseOrderId          BIGINT IDENTITY(1,1) NOT NULL,
        CompanyId                INT NOT NULL CONSTRAINT DF_tblPurchaseOrder_CompanyId DEFAULT (1),
        SiteId                   INT NOT NULL CONSTRAINT DF_tblPurchaseOrder_SiteId DEFAULT (1),
        BranchId                 INT NOT NULL CONSTRAINT DF_tblPurchaseOrder_BranchId DEFAULT (1),
        DepartmentId             INT NULL,

        PONumber                 NVARCHAR(64) NOT NULL,
        SupplierId               BIGINT NOT NULL,
        OrderDateUtc             DATETIME2 NOT NULL CONSTRAINT DF_tblPurchaseOrder_OrderDateUtc DEFAULT (GETUTCDATE()),
        ExpectedDeliveryDateUtc  DATETIME2 NOT NULL,
        CurrentStatus            INT NOT NULL CONSTRAINT DF_tblPurchaseOrder_CurrentStatus DEFAULT (2),
        ReceivedAtUtc            DATETIME2 NULL,
        SubtotalAmount           MONEY NOT NULL CONSTRAINT DF_tblPurchaseOrder_SubtotalAmount DEFAULT (0),
        TaxAmount                MONEY NOT NULL CONSTRAINT DF_tblPurchaseOrder_TaxAmount DEFAULT (0),
        TotalAmount              MONEY NOT NULL CONSTRAINT DF_tblPurchaseOrder_TotalAmount DEFAULT (0),
        Notes                    NVARCHAR(1024) NOT NULL CONSTRAINT DF_tblPurchaseOrder_Notes DEFAULT (N''),

        Tag                      NVARCHAR(64) NULL,
        Comments                 NVARCHAR(512) NULL,
        DisplayOnWeb             BIT NOT NULL CONSTRAINT DF_tblPurchaseOrder_DisplayOnWeb DEFAULT (1),
        IsPublished              BIT NOT NULL CONSTRAINT DF_tblPurchaseOrder_IsPublished DEFAULT (1),
        DatePublished            DATETIME2 NULL,
        PublishedBy              NVARCHAR(128) NULL,
        SortOrder                INT NOT NULL CONSTRAINT DF_tblPurchaseOrder_SortOrder DEFAULT (0),
        IPAddress                NVARCHAR(64) NOT NULL CONSTRAINT DF_tblPurchaseOrder_IPAddress DEFAULT (N'127.0.0.1'),
        CreatedBy                NVARCHAR(128) NOT NULL CONSTRAINT DF_tblPurchaseOrder_CreatedBy DEFAULT (N'System'),
        DateCreated              DATETIME2 NOT NULL CONSTRAINT DF_tblPurchaseOrder_DateCreated DEFAULT (GETUTCDATE()),
        UpdatedBy                NVARCHAR(128) NULL,
        LastUpdated              DATETIME2 NULL,
        DeletedBy                NVARCHAR(128) NULL,
        DateDeleted              DATETIME2 NULL,
        IsDeleted                BIT NOT NULL CONSTRAINT DF_tblPurchaseOrder_IsDeleted DEFAULT (0),

        CONSTRAINT PK_tblPurchaseOrder PRIMARY KEY (PurchaseOrderId),
        CONSTRAINT FK_tblPurchaseOrder_SupplierId_tblSupplier_SupplierId
            FOREIGN KEY (SupplierId) REFERENCES dbo.tblSupplier(SupplierId)
    );

    CREATE UNIQUE INDEX UK_tblPurchaseOrder_PONumber
        ON dbo.tblPurchaseOrder(PONumber);

    CREATE INDEX IDX_tblPurchaseOrder_CurrentStatus_ExpectedDeliveryDateUtc
        ON dbo.tblPurchaseOrder(CurrentStatus, ExpectedDeliveryDateUtc);
END

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tblPurchaseOrderItem') AND type = N'U')
BEGIN
    CREATE TABLE dbo.tblPurchaseOrderItem
    (
        PurchaseOrderItemId      BIGINT IDENTITY(1,1) NOT NULL,
        CompanyId                INT NOT NULL CONSTRAINT DF_tblPurchaseOrderItem_CompanyId DEFAULT (1),
        SiteId                   INT NOT NULL CONSTRAINT DF_tblPurchaseOrderItem_SiteId DEFAULT (1),
        BranchId                 INT NOT NULL CONSTRAINT DF_tblPurchaseOrderItem_BranchId DEFAULT (1),
        DepartmentId             INT NULL,

        PurchaseOrderId          BIGINT NOT NULL,
        ItemId                   BIGINT NOT NULL,
        PartCode                 NVARCHAR(64) NOT NULL CONSTRAINT DF_tblPurchaseOrderItem_PartCode DEFAULT (N''),
        PartName                 NVARCHAR(256) NOT NULL CONSTRAINT DF_tblPurchaseOrderItem_PartName DEFAULT (N''),
        QuantityOrdered          DECIMAL(12,2) NOT NULL CONSTRAINT DF_tblPurchaseOrderItem_QuantityOrdered DEFAULT (0),
        QuantityReceived         DECIMAL(12,2) NOT NULL CONSTRAINT DF_tblPurchaseOrderItem_QuantityReceived DEFAULT (0),
        UnitPrice                MONEY NOT NULL CONSTRAINT DF_tblPurchaseOrderItem_UnitPrice DEFAULT (0),
        Amount                   MONEY NOT NULL CONSTRAINT DF_tblPurchaseOrderItem_Amount DEFAULT (0),
        ReceivedAtUtc            DATETIME2 NULL,
        DiscrepancyFlag          BIT NOT NULL CONSTRAINT DF_tblPurchaseOrderItem_DiscrepancyFlag DEFAULT (0),

        Tag                      NVARCHAR(64) NULL,
        Comments                 NVARCHAR(512) NULL,
        DisplayOnWeb             BIT NOT NULL CONSTRAINT DF_tblPurchaseOrderItem_DisplayOnWeb DEFAULT (1),
        IsPublished              BIT NOT NULL CONSTRAINT DF_tblPurchaseOrderItem_IsPublished DEFAULT (1),
        DatePublished            DATETIME2 NULL,
        PublishedBy              NVARCHAR(128) NULL,
        SortOrder                INT NOT NULL CONSTRAINT DF_tblPurchaseOrderItem_SortOrder DEFAULT (0),
        IPAddress                NVARCHAR(64) NOT NULL CONSTRAINT DF_tblPurchaseOrderItem_IPAddress DEFAULT (N'127.0.0.1'),
        CreatedBy                NVARCHAR(128) NOT NULL CONSTRAINT DF_tblPurchaseOrderItem_CreatedBy DEFAULT (N'System'),
        DateCreated              DATETIME2 NOT NULL CONSTRAINT DF_tblPurchaseOrderItem_DateCreated DEFAULT (GETUTCDATE()),
        UpdatedBy                NVARCHAR(128) NULL,
        LastUpdated              DATETIME2 NULL,
        DeletedBy                NVARCHAR(128) NULL,
        DateDeleted              DATETIME2 NULL,
        IsDeleted                BIT NOT NULL CONSTRAINT DF_tblPurchaseOrderItem_IsDeleted DEFAULT (0),

        CONSTRAINT PK_tblPurchaseOrderItem PRIMARY KEY (PurchaseOrderItemId),
        CONSTRAINT FK_tblPurchaseOrderItem_PurchaseOrderId_tblPurchaseOrder_PurchaseOrderId
            FOREIGN KEY (PurchaseOrderId) REFERENCES dbo.tblPurchaseOrder(PurchaseOrderId),
        CONSTRAINT FK_tblPurchaseOrderItem_ItemId_tblItem_ItemId
            FOREIGN KEY (ItemId) REFERENCES dbo.tblItem(ItemId)
    );

    CREATE INDEX IDX_tblPurchaseOrderItem_PurchaseOrderId
        ON dbo.tblPurchaseOrderItem(PurchaseOrderId);

    CREATE INDEX IDX_tblPurchaseOrderItem_ItemId
        ON dbo.tblPurchaseOrderItem(ItemId);
END
