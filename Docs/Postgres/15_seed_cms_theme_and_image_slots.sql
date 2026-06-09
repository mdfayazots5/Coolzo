-- 2026-06-09
-- CMS Content & Theme Delivery (Phase 2) — seed theme.* tokens (tblSystemSetting) and starter
-- screen image slot definitions (tblScreenImageSlot). Idempotent. PostgreSQL/Supabase.

-- ---- Theme tokens (design-system defaults) ----
INSERT INTO public."tblSystemSetting" ("SettingKey", "SettingValue", "DataType", "IsSensitive", "CreatedBy", "DateCreated", "IPAddress", "IsDeleted")
SELECT v."SettingKey", v."SettingValue", 'string', FALSE, 'System', NOW(), '127.0.0.1', FALSE
FROM (VALUES
    ('theme.color.primary', '#1B2A4A'),
    ('theme.color.accent', '#C9A84C'),
    ('theme.color.background', '#FAFAFA'),
    ('theme.color.surface', '#FFFFFF'),
    ('theme.color.border', '#E2E4EE'),
    ('theme.color.textPrimary', '#1A1A2E'),
    ('theme.color.textSecondary', '#6B7080'),
    ('theme.color.success', '#27AE60'),
    ('theme.color.warning', '#F39C12'),
    ('theme.color.error', '#E74C3C'),
    ('theme.font.family', 'Inter'),
    ('theme.font.weights', '400,700'),
    ('theme.logoUrl', '')
) AS v("SettingKey", "SettingValue")
WHERE NOT EXISTS (
    SELECT 1 FROM public."tblSystemSetting" s
    WHERE s."SettingKey" = v."SettingKey" AND s."IsDeleted" = FALSE
);

-- ---- Starter screen image slots (empty ImageUrl; admin uploads real/AI images) ----
INSERT INTO public."tblScreenImageSlot"
    ("PageKey", "SlotKey", "Breakpoint", "RecommendedWidth", "RecommendedHeight", "AltText", "SuggestedAIPrompt", "ImageUrl", "IsActive", "CreatedBy", "DateCreated", "IPAddress", "IsDeleted")
SELECT v."PageKey", v."SlotKey", v."Breakpoint", v."RecommendedWidth", v."RecommendedHeight", v."AltText", v."SuggestedAIPrompt", '', TRUE, 'System', NOW(), '127.0.0.1', FALSE
FROM (VALUES
    ('home', 'hero', 'desktop', 1920, 800, 'Coolzo home hero',
     'Premium, calm hero image for an air-conditioning service brand: a bright modern Indian living room with a wall-mounted split AC, soft natural daylight, deep navy and warm gold accents, photorealistic, no text, no logos, 16:9.'),
    ('home', 'hero', 'tablet', 1024, 600, 'Coolzo home hero',
     'Same calm AC-service hero composition adapted to a 16:9-ish tablet crop, modern living room, navy/gold palette, photorealistic, no text.'),
    ('home', 'hero', 'mobile', 640, 480, 'Coolzo home hero',
     'Same calm AC-service hero subject tightly cropped for mobile portrait, focus on the wall AC and bright room, navy/gold palette, photorealistic, no text.'),
    ('services', 'banner', 'desktop', 1600, 500, 'Coolzo services banner',
     'Clean wide banner of a professional AC technician servicing a split unit, neutral background, navy and gold palette, photorealistic, no text, no logos, 16:5.'),
    ('amc', 'banner', 'desktop', 1600, 500, 'Coolzo AMC banner',
     'Reassuring wide banner conveying annual maintenance care for home cooling, subtle navy and gold tones, photorealistic, no text, 16:5.'),
    ('about', 'hero', 'desktop', 1600, 600, 'Coolzo about hero',
     'Warm, trustworthy expert-team feel for an AC service company, a subtle modern service van and clean workshop, navy and gold palette, photorealistic, no text, 8:3.')
) AS v("PageKey", "SlotKey", "Breakpoint", "RecommendedWidth", "RecommendedHeight", "AltText", "SuggestedAIPrompt")
WHERE NOT EXISTS (
    SELECT 1 FROM public."tblScreenImageSlot" t
    WHERE t."PageKey" = v."PageKey" AND t."SlotKey" = v."SlotKey" AND t."Breakpoint" = v."Breakpoint" AND t."IsDeleted" = FALSE
);
