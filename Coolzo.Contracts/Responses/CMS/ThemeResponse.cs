namespace Coolzo.Contracts.Responses.CMS;

/// <summary>Theme tokens (colors, fonts, logo) keyed by the SnapshotKeys.Theme.* registry keys.</summary>
public sealed record ThemeResponse(IReadOnlyDictionary<string, string> Tokens);
