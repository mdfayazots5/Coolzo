-- 2026-06-09
-- CMS Content & Theme Delivery (Phase 2) — admin-managed screen image slots (PostgreSQL/Supabase).
-- Aligns with Backend/Coolzo.Domain/Entities/ScreenImageSlot.cs and
-- Backend/Coolzo.Persistence/Configurations/ScreenImageSlotConfiguration.cs

CREATE TABLE IF NOT EXISTS public."tblScreenImageSlot" (
  "ScreenImageSlotId" BIGINT GENERATED ALWAYS AS IDENTITY NOT NULL,
  "CompanyId" INTEGER DEFAULT 1 NOT NULL,
  "SiteId" INTEGER DEFAULT 1 NOT NULL,
  "DepartmentId" INTEGER,
  "PageKey" VARCHAR(64) NOT NULL,
  "SlotKey" VARCHAR(64) NOT NULL,
  "Breakpoint" VARCHAR(16) NOT NULL,
  "RecommendedWidth" INTEGER DEFAULT 0 NOT NULL,
  "RecommendedHeight" INTEGER DEFAULT 0 NOT NULL,
  "AltText" VARCHAR(256) DEFAULT '' NOT NULL,
  "SuggestedAIPrompt" VARCHAR(1024) DEFAULT '' NOT NULL,
  "ImageUrl" VARCHAR(512) DEFAULT '' NOT NULL,
  "IsActive" BOOLEAN DEFAULT TRUE NOT NULL,
  "Tag" VARCHAR(64),
  "Comments" VARCHAR(512),
  "DisplayOnWeb" BOOLEAN DEFAULT TRUE NOT NULL,
  "IsPublished" BOOLEAN DEFAULT TRUE NOT NULL,
  "DatePublished" TIMESTAMPTZ,
  "PublishedBy" VARCHAR(128),
  "SortOrder" INTEGER DEFAULT 0 NOT NULL,
  "IPAddress" VARCHAR(64) DEFAULT '127.0.0.1' NOT NULL,
  "CreatedBy" VARCHAR(128) DEFAULT 'System' NOT NULL,
  "DateCreated" TIMESTAMPTZ DEFAULT NOW() NOT NULL,
  "UpdatedBy" VARCHAR(128),
  "LastUpdated" TIMESTAMPTZ,
  "DeletedBy" VARCHAR(128),
  "DateDeleted" TIMESTAMPTZ,
  "IsDeleted" BOOLEAN DEFAULT FALSE NOT NULL,
  "BranchId" INTEGER DEFAULT 1 NOT NULL,
  CONSTRAINT "PK_tblScreenImageSlot_ScreenImageSlotId" PRIMARY KEY ("ScreenImageSlotId")
);

CREATE UNIQUE INDEX IF NOT EXISTS "UK_tblScreenImageSlot_PageKey_SlotKey_Breakpoint"
  ON public."tblScreenImageSlot" ("PageKey", "SlotKey", "Breakpoint");
