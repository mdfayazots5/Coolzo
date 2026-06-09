using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Coolzo.Infrastructure.Storage;

/// <summary>
/// Filesystem-backed object storage. Works for local development (under wwwroot, served by static
/// files) and for a Render persistent disk (absolute mounted RootPath). Object keys are relative,
/// forward-slash separated paths, e.g. "cms/snapshot-latest.json".
/// </summary>
public sealed class FileSystemObjectStorageService : IObjectStorageService
{
    private const string HealthProbeKey = "cms/.storage-health-probe";

    private readonly IHostEnvironment _hostEnvironment;
    private readonly ObjectStorageOptions _options;

    public FileSystemObjectStorageService(
        IHostEnvironment hostEnvironment,
        IOptions<ObjectStorageOptions> options)
    {
        _hostEnvironment = hostEnvironment;
        _options = options.Value;
    }

    public async Task<StoredObjectResult> PutObjectAsync(
        string key,
        string contentType,
        byte[] content,
        CancellationToken cancellationToken)
    {
        var filePath = ResolveObjectPath(key);
        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(filePath, content, cancellationToken);

        return new StoredObjectResult(key, GetPublicUrl(key), content.LongLength, contentType);
    }

    public async Task<byte[]?> GetObjectAsync(string key, CancellationToken cancellationToken)
    {
        var filePath = ResolveObjectPath(key);

        if (!File.Exists(filePath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(filePath, cancellationToken);
    }

    public Task<bool> ObjectExistsAsync(string key, CancellationToken cancellationToken)
    {
        var filePath = ResolveObjectPath(key);
        return Task.FromResult(File.Exists(filePath));
    }

    public string GetPublicUrl(string key)
    {
        var normalizedKey = NormalizeKey(key);

        return string.IsNullOrWhiteSpace(_options.PublicBaseUrl)
            ? $"/{normalizedKey}"
            : $"{_options.PublicBaseUrl.TrimEnd('/')}/{normalizedKey}";
    }

    public async Task<bool> CheckWritableAsync(CancellationToken cancellationToken)
    {
        try
        {
            var probe = new byte[] { 0x6F, 0x6B };
            await PutObjectAsync(HealthProbeKey, "application/octet-stream", probe, cancellationToken);

            var probePath = ResolveObjectPath(HealthProbeKey);
            File.Delete(probePath);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private string ResolveObjectPath(string key)
    {
        var normalizedKey = NormalizeKey(key);
        var relativePath = normalizedKey.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(ResolveRoot(), relativePath);
    }

    private string ResolveRoot()
    {
        var root = string.IsNullOrWhiteSpace(_options.RootPath) ? "wwwroot" : _options.RootPath;

        return Path.IsPathRooted(root)
            ? root
            : Path.Combine(_hostEnvironment.ContentRootPath, root);
    }

    private static string NormalizeKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Object storage key must not be empty.", nameof(key));
        }

        var trimmed = key.Replace('\\', '/').TrimStart('/');

        if (trimmed.Contains("..", StringComparison.Ordinal) || Path.IsPathRooted(trimmed))
        {
            throw new ArgumentException("Object storage key must be a relative path without traversal.", nameof(key));
        }

        return trimmed;
    }
}
