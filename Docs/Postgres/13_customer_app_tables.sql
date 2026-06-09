-- ============================================================
-- Migration : 13_customer_app_tables.sql
-- Date       : 2026-05-26
-- Author     : System
-- Purpose    : Create missing CustomerApp tables that were defined
--              in EF Core but never applied to the PostgreSQL schema.
--              Fixes 500 on GET /api/customer-notifications.
-- Tables     : tblCustomerNotification
--              tblPromotionalOffer
--              tblCustomerReferral
--              tblCustomerLoyaltyTransaction
--              tblCustomerAppFeedback
-- ============================================================

-- ── 1. tblCustomerNotification ──────────────────────────────
CREATE TABLE IF NOT EXISTS public."tblCustomerNotification" (
  "CustomerNotificationId"  BIGINT      GENERATED ALWAYS AS IDENTITY NOT NULL,
  "CompanyId"               INTEGER     DEFAULT 1 NOT NULL,
  "SiteId"                  INTEGER     DEFAULT 1 NOT NULL,
  "BranchId"                INTEGER     DEFAULT 1 NOT NULL,
  "DepartmentId"            INTEGER,
  "CustomerId"              BIGINT      NOT NULL,
  "Title"                   VARCHAR(160) NOT NULL,
  "Message"                 VARCHAR(1000) NOT NULL,
  "NotificationType"        VARCHAR(32)  DEFAULT 'info' NOT NULL,
  "IsRead"                  BOOLEAN     DEFAULT FALSE NOT NULL,
  "LinkUrl"                 VARCHAR(512) DEFAULT '' NOT NULL,
  "Tag"                     VARCHAR(64),
  "Comments"                VARCHAR(512),
  "DisplayOnWeb"            BOOLEAN     DEFAULT TRUE NOT NULL,
  "IsPublished"             BOOLEAN     DEFAULT TRUE NOT NULL,
  "DatePublished"           TIMESTAMPTZ,
  "PublishedBy"             VARCHAR(128),
  "SortOrder"               INTEGER     DEFAULT 0 NOT NULL,
  "IPAddress"               VARCHAR(64)  DEFAULT '127.0.0.1' NOT NULL,
  "CreatedBy"               VARCHAR(128) DEFAULT 'Admin' NOT NULL,
  "DateCreated"             TIMESTAMPTZ  DEFAULT NOW() NOT NULL,
  "UpdatedBy"               VARCHAR(128),
  "LastUpdated"             TIMESTAMPTZ,
  "DeletedBy"               VARCHAR(128),
  "DateDeleted"             TIMESTAMPTZ,
  "IsDeleted"               BOOLEAN     DEFAULT FALSE NOT NULL,
  CONSTRAINT "PK_tblCustomerNotification_CustomerNotificationId"
    PRIMARY KEY ("CustomerNotificationId")
);

ALTER TABLE public."tblCustomerNotification"
  ADD CONSTRAINT "FK_tblCustomerNotification_CustomerId_tblCustomer_CustomerId"
  FOREIGN KEY ("CustomerId")
  REFERENCES public."tblCustomer" ("CustomerId")
  ON DELETE RESTRICT;

CREATE INDEX IF NOT EXISTS "IDX_tblCustomerNotification_CustomerId_IsRead_DateCreated"
  ON public."tblCustomerNotification" ("CustomerId", "IsRead", "DateCreated");


-- ── 2. tblPromotionalOffer ───────────────────────────────────
CREATE TABLE IF NOT EXISTS public."tblPromotionalOffer" (
  "PromotionalOfferId"      BIGINT       GENERATED ALWAYS AS IDENTITY NOT NULL,
  "CompanyId"               INTEGER      DEFAULT 1 NOT NULL,
  "SiteId"                  INTEGER      DEFAULT 1 NOT NULL,
  "BranchId"                INTEGER      DEFAULT 1 NOT NULL,
  "DepartmentId"            INTEGER,
  "OfferCode"               VARCHAR(64)  NOT NULL,
  "Title"                   VARCHAR(160) NOT NULL,
  "Description"             VARCHAR(1000) DEFAULT '' NOT NULL,
  "DiscountType"            VARCHAR(32)  DEFAULT 'fixed' NOT NULL,
  "DiscountValue"           NUMERIC(18,2) DEFAULT 0 NOT NULL,
  "MinimumOrderValue"       NUMERIC(18,2) DEFAULT 0 NOT NULL,
  "ExpiryDate"              DATE,
  "Category"                VARCHAR(64)  DEFAULT '' NOT NULL,
  "IsActive"                BOOLEAN      DEFAULT TRUE NOT NULL,
  "Tag"                     VARCHAR(64),
  "Comments"                VARCHAR(512),
  "DisplayOnWeb"            BOOLEAN      DEFAULT TRUE NOT NULL,
  "IsPublished"             BOOLEAN      DEFAULT TRUE NOT NULL,
  "DatePublished"           TIMESTAMPTZ,
  "PublishedBy"             VARCHAR(128),
  "SortOrder"               INTEGER      DEFAULT 0 NOT NULL,
  "IPAddress"               VARCHAR(64)  DEFAULT '127.0.0.1' NOT NULL,
  "CreatedBy"               VARCHAR(128) DEFAULT 'Admin' NOT NULL,
  "DateCreated"             TIMESTAMPTZ  DEFAULT NOW() NOT NULL,
  "UpdatedBy"               VARCHAR(128),
  "LastUpdated"             TIMESTAMPTZ,
  "DeletedBy"               VARCHAR(128),
  "DateDeleted"             TIMESTAMPTZ,
  "IsDeleted"               BOOLEAN      DEFAULT FALSE NOT NULL,
  CONSTRAINT "PK_tblPromotionalOffer_PromotionalOfferId"
    PRIMARY KEY ("PromotionalOfferId")
);

CREATE UNIQUE INDEX IF NOT EXISTS "UK_tblPromotionalOffer_OfferCode"
  ON public."tblPromotionalOffer" ("OfferCode")
  WHERE "IsDeleted" = FALSE;


-- ── 3. tblCustomerReferral ───────────────────────────────────
CREATE TABLE IF NOT EXISTS public."tblCustomerReferral" (
  "CustomerReferralId"      BIGINT       GENERATED ALWAYS AS IDENTITY NOT NULL,
  "CompanyId"               INTEGER      DEFAULT 1 NOT NULL,
  "SiteId"                  INTEGER      DEFAULT 1 NOT NULL,
  "BranchId"                INTEGER      DEFAULT 1 NOT NULL,
  "DepartmentId"            INTEGER,
  "CustomerId"              BIGINT       NOT NULL,
  "ReferralName"            VARCHAR(128) DEFAULT '' NOT NULL,
  "ReferralStatus"          VARCHAR(32)  DEFAULT 'Pending' NOT NULL,
  "RewardAmount"            NUMERIC(18,2) DEFAULT 0 NOT NULL,
  "ReferralDate"            DATE         NOT NULL,
  "Tag"                     VARCHAR(64),
  "Comments"                VARCHAR(512),
  "DisplayOnWeb"            BOOLEAN      DEFAULT TRUE NOT NULL,
  "IsPublished"             BOOLEAN      DEFAULT TRUE NOT NULL,
  "DatePublished"           TIMESTAMPTZ,
  "PublishedBy"             VARCHAR(128),
  "SortOrder"               INTEGER      DEFAULT 0 NOT NULL,
  "IPAddress"               VARCHAR(64)  DEFAULT '127.0.0.1' NOT NULL,
  "CreatedBy"               VARCHAR(128) DEFAULT 'Admin' NOT NULL,
  "DateCreated"             TIMESTAMPTZ  DEFAULT NOW() NOT NULL,
  "UpdatedBy"               VARCHAR(128),
  "LastUpdated"             TIMESTAMPTZ,
  "DeletedBy"               VARCHAR(128),
  "DateDeleted"             TIMESTAMPTZ,
  "IsDeleted"               BOOLEAN      DEFAULT FALSE NOT NULL,
  CONSTRAINT "PK_tblCustomerReferral_CustomerReferralId"
    PRIMARY KEY ("CustomerReferralId")
);

ALTER TABLE public."tblCustomerReferral"
  ADD CONSTRAINT "FK_tblCustomerReferral_CustomerId_tblCustomer_CustomerId"
  FOREIGN KEY ("CustomerId")
  REFERENCES public."tblCustomer" ("CustomerId")
  ON DELETE RESTRICT;

CREATE INDEX IF NOT EXISTS "IDX_tblCustomerReferral_CustomerId_ReferralDate"
  ON public."tblCustomerReferral" ("CustomerId", "ReferralDate");


-- ── 4. tblCustomerLoyaltyTransaction ─────────────────────────
CREATE TABLE IF NOT EXISTS public."tblCustomerLoyaltyTransaction" (
  "CustomerLoyaltyTransactionId" BIGINT      GENERATED ALWAYS AS IDENTITY NOT NULL,
  "CompanyId"                    INTEGER     DEFAULT 1 NOT NULL,
  "SiteId"                       INTEGER     DEFAULT 1 NOT NULL,
  "BranchId"                     INTEGER     DEFAULT 1 NOT NULL,
  "DepartmentId"                 INTEGER,
  "CustomerId"                   BIGINT      NOT NULL,
  "TransactionType"              VARCHAR(32) DEFAULT 'earn' NOT NULL,
  "Points"                       INTEGER     DEFAULT 0 NOT NULL,
  "Description"                  VARCHAR(512) DEFAULT '' NOT NULL,
  "Tag"                          VARCHAR(64),
  "Comments"                     VARCHAR(512),
  "DisplayOnWeb"                 BOOLEAN     DEFAULT TRUE NOT NULL,
  "IsPublished"                  BOOLEAN     DEFAULT TRUE NOT NULL,
  "DatePublished"                TIMESTAMPTZ,
  "PublishedBy"                  VARCHAR(128),
  "SortOrder"                    INTEGER     DEFAULT 0 NOT NULL,
  "IPAddress"                    VARCHAR(64) DEFAULT '127.0.0.1' NOT NULL,
  "CreatedBy"                    VARCHAR(128) DEFAULT 'Admin' NOT NULL,
  "DateCreated"                  TIMESTAMPTZ DEFAULT NOW() NOT NULL,
  "UpdatedBy"                    VARCHAR(128),
  "LastUpdated"                  TIMESTAMPTZ,
  "DeletedBy"                    VARCHAR(128),
  "DateDeleted"                  TIMESTAMPTZ,
  "IsDeleted"                    BOOLEAN     DEFAULT FALSE NOT NULL,
  CONSTRAINT "PK_tblCustomerLoyaltyTransaction_CustomerLoyaltyTransactionId"
    PRIMARY KEY ("CustomerLoyaltyTransactionId")
);

ALTER TABLE public."tblCustomerLoyaltyTransaction"
  ADD CONSTRAINT "FK_tblCustomerLoyaltyTransaction_CustomerId_tblCustomer_CustomerId"
  FOREIGN KEY ("CustomerId")
  REFERENCES public."tblCustomer" ("CustomerId")
  ON DELETE RESTRICT;

CREATE INDEX IF NOT EXISTS "IDX_tblCustomerLoyaltyTransaction_CustomerId_DateCreated"
  ON public."tblCustomerLoyaltyTransaction" ("CustomerId", "DateCreated");


-- ── 5. tblCustomerAppFeedback ─────────────────────────────────
CREATE TABLE IF NOT EXISTS public."tblCustomerAppFeedback" (
  "CustomerAppFeedbackId"   BIGINT       GENERATED ALWAYS AS IDENTITY NOT NULL,
  "CompanyId"               INTEGER      DEFAULT 1 NOT NULL,
  "SiteId"                  INTEGER      DEFAULT 1 NOT NULL,
  "BranchId"                INTEGER      DEFAULT 1 NOT NULL,
  "DepartmentId"            INTEGER,
  "CustomerId"              BIGINT       NOT NULL,
  "FeedbackType"            VARCHAR(64)  DEFAULT 'general' NOT NULL,
  "Message"                 VARCHAR(2000) NOT NULL,
  "Rating"                  INTEGER,
  "AppVersion"              VARCHAR(32)  DEFAULT '' NOT NULL,
  "DeviceInfo"              VARCHAR(256) DEFAULT '' NOT NULL,
  "FeedbackStatus"          VARCHAR(32)  DEFAULT 'Submitted' NOT NULL,
  "Tag"                     VARCHAR(64),
  "Comments"                VARCHAR(512),
  "DisplayOnWeb"            BOOLEAN      DEFAULT TRUE NOT NULL,
  "IsPublished"             BOOLEAN      DEFAULT TRUE NOT NULL,
  "DatePublished"           TIMESTAMPTZ,
  "PublishedBy"             VARCHAR(128),
  "SortOrder"               INTEGER      DEFAULT 0 NOT NULL,
  "IPAddress"               VARCHAR(64)  DEFAULT '127.0.0.1' NOT NULL,
  "CreatedBy"               VARCHAR(128) DEFAULT 'Admin' NOT NULL,
  "DateCreated"             TIMESTAMPTZ  DEFAULT NOW() NOT NULL,
  "UpdatedBy"               VARCHAR(128),
  "LastUpdated"             TIMESTAMPTZ,
  "DeletedBy"               VARCHAR(128),
  "DateDeleted"             TIMESTAMPTZ,
  "IsDeleted"               BOOLEAN      DEFAULT FALSE NOT NULL,
  CONSTRAINT "PK_tblCustomerAppFeedback_CustomerAppFeedbackId"
    PRIMARY KEY ("CustomerAppFeedbackId")
);

ALTER TABLE public."tblCustomerAppFeedback"
  ADD CONSTRAINT "FK_tblCustomerAppFeedback_CustomerId_tblCustomer_CustomerId"
  FOREIGN KEY ("CustomerId")
  REFERENCES public."tblCustomer" ("CustomerId")
  ON DELETE RESTRICT;

CREATE INDEX IF NOT EXISTS "IDX_tblCustomerAppFeedback_CustomerId_DateCreated"
  ON public."tblCustomerAppFeedback" ("CustomerId", "DateCreated");
