namespace Coolzo.Application.Common.Models;

public sealed record StoredObjectResult(
    string Key,
    string PublicUrl,
    long SizeInBytes,
    string ContentType);
