using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using Microsoft.Extensions.Hosting;

namespace Coolzo.Infrastructure.Storage;

public sealed class LocalJobAttachmentStorageService : IJobAttachmentStorageService
{
    private readonly IHostEnvironment _hostEnvironment;

    public LocalJobAttachmentStorageService(IHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    public async Task<StoredJobAttachmentResult> SaveAsync(
        string fileName,
        string contentType,
        byte[] fileBytes,
        CancellationToken cancellationToken)
    {
        var safeExtension = Path.GetExtension(fileName);
        var storedFileName = $"{Guid.NewGuid():N}{safeExtension}".ToLowerInvariant();
        var relativeFolder = Path.Combine("uploads", "job-attachments", DateTime.UtcNow.ToString("yyyyMMdd"));
        var rootFolder = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot");
        var targetFolder = Path.Combine(rootFolder, relativeFolder);
        Directory.CreateDirectory(targetFolder);

        var filePath = Path.Combine(targetFolder, storedFileName);
        await File.WriteAllBytesAsync(filePath, fileBytes, cancellationToken);

        return new StoredJobAttachmentResult(
            storedFileName,
            $"/{relativeFolder.Replace('\\', '/')}/{storedFileName}",
            fileBytes.LongLength);
    }
}
