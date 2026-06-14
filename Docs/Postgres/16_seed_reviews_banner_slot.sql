-- 2026-06-14
-- Website Rework (item 14) — seed the reviews.banner screen image slot so the public /reviews
-- banner (Reviews.tsx <SnapshotImage slotKey="reviews.banner">) becomes admin-managed instead of
-- showing only the bundled fallback. Idempotent. PostgreSQL/Supabase.

INSERT INTO public."tblScreenImageSlot"
    ("PageKey", "SlotKey", "Breakpoint", "RecommendedWidth", "RecommendedHeight", "AltText", "SuggestedAIPrompt", "ImageUrl", "IsActive", "CreatedBy", "DateCreated", "IPAddress", "IsDeleted")
SELECT v."PageKey", v."SlotKey", v."Breakpoint", v."RecommendedWidth", v."RecommendedHeight", v."AltText", v."SuggestedAIPrompt", '', TRUE, 'System', NOW(), '127.0.0.1', FALSE
FROM (VALUES
    ('reviews', 'banner', 'desktop', 1600, 500, 'Coolzo customer reviews banner',
     'Warm, trustworthy wide banner conveying happy customers and verified five-star AC service, subtle navy and gold tones, photorealistic, no text, no logos, 16:5.')
) AS v("PageKey", "SlotKey", "Breakpoint", "RecommendedWidth", "RecommendedHeight", "AltText", "SuggestedAIPrompt")
WHERE NOT EXISTS (
    SELECT 1 FROM public."tblScreenImageSlot" t
    WHERE t."PageKey" = v."PageKey" AND t."SlotKey" = v."SlotKey" AND t."Breakpoint" = v."Breakpoint" AND t."IsDeleted" = FALSE
);
