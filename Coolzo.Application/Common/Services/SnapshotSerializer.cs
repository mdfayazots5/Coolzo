using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Coolzo.Contracts.Responses.CMS;

namespace Coolzo.Application.Common.Services;

/// <summary>
/// Canonical (camelCase) serialization and integrity hashing for the published content snapshot.
/// The portal consumes this exact JSON shape.
/// </summary>
public static class SnapshotSerializer
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = null,
        WriteIndented = false
    };

    public static byte[] Serialize(ContentSnapshotResponse snapshot)
    {
        return JsonSerializer.SerializeToUtf8Bytes(snapshot, Options);
    }

    public static ContentSnapshotResponse? Deserialize(byte[] payload)
    {
        return JsonSerializer.Deserialize<ContentSnapshotResponse>(payload, Options);
    }

    public static string ComputeChecksum(byte[] payload)
    {
        var hash = SHA256.HashData(payload);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static string ComputeBodyChecksum(ContentSnapshotBody body)
    {
        var bodyBytes = JsonSerializer.SerializeToUtf8Bytes(body, Options);
        return ComputeChecksum(bodyBytes);
    }
}
