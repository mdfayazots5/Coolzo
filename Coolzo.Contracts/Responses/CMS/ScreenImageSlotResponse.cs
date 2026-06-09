namespace Coolzo.Contracts.Responses.CMS;

public sealed record ScreenImageSlotResponse(
    long ScreenImageSlotId,
    string PageKey,
    string SlotKey,
    string Breakpoint,
    int RecommendedWidth,
    int RecommendedHeight,
    string AltText,
    string SuggestedAIPrompt,
    string ImageUrl,
    bool IsActive);
