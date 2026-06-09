namespace Coolzo.Contracts.Requests.CMS;

/// <summary>Partial or full theme token update. Keys must belong to the SnapshotKeys.Theme.* registry.</summary>
public sealed record UpdateThemeRequest(IReadOnlyDictionary<string, string> Tokens);
