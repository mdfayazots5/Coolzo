namespace Coolzo.Domain.Entities;

/// <summary>
/// An admin-managed image placement on the public Web portal. Identified by PageKey + SlotKey +
/// Breakpoint, so a single logical slot (e.g. home.hero) can carry desktop/tablet/mobile variants.
/// Carries a suggested AI prompt the admin can use to generate the image in Gemini.
/// </summary>
public sealed class ScreenImageSlot : AuditableEntity
{
    public long ScreenImageSlotId { get; set; }

    public string PageKey { get; set; } = string.Empty;

    public string SlotKey { get; set; } = string.Empty;

    public string Breakpoint { get; set; } = string.Empty;

    public int RecommendedWidth { get; set; }

    public int RecommendedHeight { get; set; }

    public string AltText { get; set; } = string.Empty;

    public string SuggestedAIPrompt { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
