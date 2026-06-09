-- 2026-06-09
-- CMS Content & Theme Delivery (Phase 1) — published content snapshot version registry (PostgreSQL/Supabase).
-- Aligns with Backend/Coolzo.Domain/Entities/PublishedSnapshot.cs and
-- Backend/Coolzo.Persistence/Configurations/PublishedSnapshotConfiguration.cs

CREATE TABLE IF NOT EXISTS public."tblPublishedSnapshot" (
  "PublishedSnapshotId" BIGINT GENERATED ALWAYS AS IDENTITY NOT NULL,
  "CompanyId" INTEGER DEFAULT 1 NOT NULL,
  "SiteId" INTEGER DEFAULT 1 NOT NULL,
  "DepartmentId" INTEGER,
  "Version" INTEGER NOT NULL,
  "BucketKey" VARCHAR(256) DEFAULT '' NOT NULL,
  "BucketUrl" VARCHAR(512) DEFAULT '' NOT NULL,
  "ChecksumHash" VARCHAR(128) DEFAULT '' NOT NULL,
  "PayloadSizeBytes" BIGINT DEFAULT 0 NOT NULL,
  "IsActive" BOOLEAN DEFAULT FALSE NOT NULL,
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
  CONSTRAINT "PK_tblPublishedSnapshot_PublishedSnapshotId" PRIMARY KEY ("PublishedSnapshotId")
);

CREATE UNIQUE INDEX IF NOT EXISTS "UK_tblPublishedSnapshot_Version"
  ON public."tblPublishedSnapshot" ("Version");

CREATE INDEX IF NOT EXISTS "IDX_tblPublishedSnapshot_IsActive"
  ON public."tblPublishedSnapshot" ("IsActive");
