using Coolzo.Contracts.Responses.CMS;

namespace Coolzo.Application.Common.Interfaces;

/// <summary>
/// Aggregates the current published masters, content, theme, and images into a snapshot body.
/// The publish handler wraps the body in the version/checksum envelope.
/// </summary>
public interface IContentSnapshotBuilder
{
    Task<ContentSnapshotBody> BuildAsync(CancellationToken cancellationToken);
}
