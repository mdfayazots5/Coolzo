namespace Coolzo.Contracts.Requests.Admin;

/// <summary>
/// Sets (or clears, when null/empty) the per-service AI image-generation prompt on a bookable
/// tblService row. Stored so the admin can re-generate the service image from a tuned, reusable
/// prompt without rewriting it each time. Null/empty clears the override (UI falls back to a
/// generated suggested prompt).
/// </summary>
public sealed record SetServicePromptRequest(string? ImageAIPrompt);
