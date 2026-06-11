SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-------------------------------------------------------------------------------------------------------------
-- Created By   : Coolzo System
-- Date Created : 09 Jun 2026
-- Description  : Seed CMS theme.* tokens (tblSystemSetting) and starter screen image slots
--                (tblScreenImageSlot). Idempotent. Run once against the target database.
-------------------------------------------------------------------------------------------------------------

-- ---- Theme tokens (design-system defaults) ----
INSERT INTO dbo.tblSystemSetting (SettingKey, SettingValue, DataType, IsSensitive, CreatedBy, DateCreated, IPAddress, IsDeleted)
SELECT v.SettingKey, v.SettingValue, N'string', 0, N'System', GETUTCDATE(), N'127.0.0.1', 0
FROM (VALUES
    (N'theme.color.primary', N'#1B2A4A'),
    (N'theme.color.accent', N'#C9A84C'),
    (N'theme.color.background', N'#FAFAFA'),
    (N'theme.color.surface', N'#FFFFFF'),
    (N'theme.color.border', N'#E2E4EE'),
    (N'theme.color.textPrimary', N'#1A1A2E'),
    (N'theme.color.textSecondary', N'#6B7080'),
    (N'theme.color.success', N'#27AE60'),
    (N'theme.color.warning', N'#F39C12'),
    (N'theme.color.error', N'#E74C3C'),
    (N'theme.font.family', N'Inter'),
    (N'theme.font.weights', N'400,700'),
    (N'theme.logoUrl', N'')
) AS v(SettingKey, SettingValue)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.tblSystemSetting s WHERE s.SettingKey = v.SettingKey AND s.IsDeleted = 0
);
GO

-- ---- Starter screen image slots (empty ImageUrl; admin uploads real/AI images) ----
INSERT INTO dbo.tblScreenImageSlot
    (PageKey, SlotKey, Breakpoint, RecommendedWidth, RecommendedHeight, AltText, SuggestedAIPrompt, ImageUrl, IsActive, CreatedBy, DateCreated, IPAddress, IsDeleted)
SELECT v.PageKey, v.SlotKey, v.Breakpoint, v.RecommendedWidth, v.RecommendedHeight, v.AltText, v.SuggestedAIPrompt, N'', 1, N'System', GETUTCDATE(), N'127.0.0.1', 0
FROM (VALUES
    (N'home', N'hero', N'desktop', 1920, 800, N'Coolzo home hero',
     N'Premium, calm hero image for an air-conditioning service brand: a bright modern Indian living room with a wall-mounted split AC, soft natural daylight, deep navy and warm gold accents, photorealistic, no text, no logos, 16:9.'),
    (N'home', N'hero', N'tablet', 1024, 600, N'Coolzo home hero',
     N'Same calm AC-service hero composition adapted to a tablet crop, modern living room, navy/gold palette, photorealistic, no text.'),
    (N'home', N'hero', N'mobile', 640, 480, N'Coolzo home hero',
     N'Same calm AC-service hero subject tightly cropped for mobile portrait, focus on the wall AC and bright room, navy/gold palette, photorealistic, no text.'),
    (N'services', N'banner', N'desktop', 1600, 500, N'Coolzo services banner',
     N'Clean wide banner of a professional AC technician servicing a split unit, neutral background, navy and gold palette, photorealistic, no text, no logos, 16:5.'),
    (N'amc', N'banner', N'desktop', 1600, 500, N'Coolzo AMC banner',
     N'Reassuring wide banner conveying annual maintenance care for home cooling, subtle navy and gold tones, photorealistic, no text, 16:5.'),
    (N'about', N'hero', N'desktop', 1600, 600, N'Coolzo about hero',
     N'Warm, trustworthy expert-team feel for an AC service company, a subtle modern service van and clean workshop, navy and gold palette, photorealistic, no text, 8:3.'),
    (N'home', N'coverage', N'desktop', 1200, 900, N'Hyderabad service coverage',
     N'Trustworthy image of an AC service technician with a branded service van in a Hyderabad residential neighbourhood, clear daylight, navy and gold palette, photorealistic, no text, no logos, 4:3.')
) AS v(PageKey, SlotKey, Breakpoint, RecommendedWidth, RecommendedHeight, AltText, SuggestedAIPrompt)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.tblScreenImageSlot t
    WHERE t.PageKey = v.PageKey AND t.SlotKey = v.SlotKey AND t.Breakpoint = v.Breakpoint AND t.IsDeleted = 0
);
GO
