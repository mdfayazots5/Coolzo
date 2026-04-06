namespace Coolzo.Application.Common.Models;

public sealed record SupportLinkedEntityResolution
(
    long CustomerId,
    string LinkReference,
    string LinkSummary
);
