using Coolzo.Application.Common.Models;

namespace Coolzo.Application.Common.Interfaces;

public interface IJobAttachmentStorageService
{
    Task<StoredJobAttachmentResult> SaveAsync(
        string fileName,
        string contentType,
        byte[] fileBytes,
        CancellationToken cancellationToken);
}
