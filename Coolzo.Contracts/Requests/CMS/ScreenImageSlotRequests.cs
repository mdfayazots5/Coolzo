namespace Coolzo.Contracts.Requests.CMS;

public sealed record ScreenImageSlotUpsertRequest(
    string PageKey,
    string SlotKey,
    string Breakpoint,
    int RecommendedWidth,
    int RecommendedHeight,
    string? AltText,
    string? SuggestedAIPrompt,
    bool IsActive);

public sealed record ScreenImageUploadRequest(
    string FileName,
    string ContentType,
    string Base64Content,
    string? AltText);
