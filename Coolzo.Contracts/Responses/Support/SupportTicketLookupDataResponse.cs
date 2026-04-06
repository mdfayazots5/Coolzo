using Coolzo.Contracts.Common;

namespace Coolzo.Contracts.Responses.Support;

public sealed record SupportTicketLookupDataResponse
(
    IReadOnlyCollection<LookupItemResponse> Categories,
    IReadOnlyCollection<LookupItemResponse> Priorities,
    IReadOnlyCollection<LookupItemResponse> Statuses
);
