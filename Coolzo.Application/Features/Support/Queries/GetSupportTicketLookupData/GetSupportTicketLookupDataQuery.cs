using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Queries.GetSupportTicketLookupData;

public sealed record GetSupportTicketLookupDataQuery : IRequest<SupportTicketLookupDataResponse>;
