IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.tblTechnicianZone')
      AND name = N'UK_tblTechnicianZone_TechnicianId_ZoneId'
)
BEGIN
    DROP INDEX UK_tblTechnicianZone_TechnicianId_ZoneId ON dbo.tblTechnicianZone;
END;

CREATE UNIQUE INDEX UK_tblTechnicianZone_TechnicianId_ZoneId
    ON dbo.tblTechnicianZone (TechnicianId, ZoneId)
    WHERE IsDeleted = 0;

IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.tblTechnicianSkill')
      AND name = N'UK_tblTechnicianSkill_TechnicianId_SkillName'
)
BEGIN
    DROP INDEX UK_tblTechnicianSkill_TechnicianId_SkillName ON dbo.tblTechnicianSkill;
END;

CREATE UNIQUE INDEX UK_tblTechnicianSkill_TechnicianId_SkillName
    ON dbo.tblTechnicianSkill (TechnicianId, SkillName)
    WHERE IsDeleted = 0;
