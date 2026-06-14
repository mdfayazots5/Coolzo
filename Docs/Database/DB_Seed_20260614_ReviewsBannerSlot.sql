SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-------------------------------------------------------------------------------------------------------------
-- Created By   : Coolzo System
-- Date Created : 14 Jun 2026
-- Description  : Website Rework (item 14) — seed the reviews.banner screen image slot so the public
--                /reviews banner becomes admin-managed (not just the bundled fallback). Idempotent.
-------------------------------------------------------------------------------------------------------------

INSERT INTO dbo.tblScreenImageSlot
    (PageKey, SlotKey, Breakpoint, RecommendedWidth, RecommendedHeight, AltText, SuggestedAIPrompt, ImageUrl, IsActive, CreatedBy, DateCreated, IPAddress, IsDeleted)
SELECT v.PageKey, v.SlotKey, v.Breakpoint, v.RecommendedWidth, v.RecommendedHeight, v.AltText, v.SuggestedAIPrompt, N'', 1, N'System', GETUTCDATE(), N'127.0.0.1', 0
FROM (VALUES
    (N'reviews', N'banner', N'desktop', 1600, 500, N'Coolzo customer reviews banner',
     N'Warm, trustworthy wide banner conveying happy customers and verified five-star AC service, subtle navy and gold tones, photorealistic, no text, no logos, 16:5.')
) AS v(PageKey, SlotKey, Breakpoint, RecommendedWidth, RecommendedHeight, AltText, SuggestedAIPrompt)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.tblScreenImageSlot t
    WHERE t.PageKey = v.PageKey AND t.SlotKey = v.SlotKey AND t.Breakpoint = v.Breakpoint AND t.IsDeleted = 0
);
GO
