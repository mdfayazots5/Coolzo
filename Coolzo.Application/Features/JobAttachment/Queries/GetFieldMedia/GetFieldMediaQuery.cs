using Coolzo.Application.Common.Models;
using MediatR;

namespace Coolzo.Application.Features.JobAttachment.Queries.GetFieldMedia;

/// <summary>
/// Streams a job-media object (stored in the private R2 bucket) by its storage key. Backs the
/// /api/field-media proxy so the bytes are reachable without exposing the bucket publicly.
/// </summary>
public sealed record GetFieldMediaQuery(string ObjectKey) : IRequest<JobMediaContent?>;
