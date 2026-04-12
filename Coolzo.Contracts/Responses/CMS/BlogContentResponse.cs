namespace Coolzo.Contracts.Responses.CMS;

public sealed record BlogContentResponse(
    string Id,
    string Title,
    string Excerpt,
    string Content,
    string Author,
    DateTime Date,
    string Image,
    string Category);

public sealed record ChangelogItemResponse(
    string Version,
    DateTime Date,
    IReadOnlyCollection<string> Changes);
