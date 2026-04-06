using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Support;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Support.Queries.GetSupportTicketList;

public sealed class GetSupportTicketListQueryHandler : IRequestHandler<GetSupportTicketListQuery, PagedResult<SupportTicketListItemResponse>>
{
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly SupportTicketAccessService _supportTicketAccessService;

    public GetSupportTicketListQueryHandler(
        ISupportTicketRepository supportTicketRepository,
        SupportTicketAccessService supportTicketAccessService)
    {
        _supportTicketRepository = supportTicketRepository;
        _supportTicketAccessService = supportTicketAccessService;
    }

    public async Task<PagedResult<SupportTicketListItemResponse>> Handle(GetSupportTicketListQuery request, CancellationToken cancellationToken)
    {
        if (!_supportTicketAccessService.CanReadAll())
        {
            throw new AppException(ErrorCodes.SupportAccessDenied, "The current user is not allowed to search all support tickets.", 403);
        }

        var status = ParseStatus(request.Status);
        var linkedEntityType = ParseLinkedEntityType(request.LinkedEntityType);
        var tickets = await _supportTicketRepository.SearchAsync(
            request.TicketNumber,
            request.CustomerMobile,
            request.CategoryId,
            request.PriorityId,
            status,
            request.DateFrom,
            request.DateTo,
            linkedEntityType,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
        var totalCount = await _supportTicketRepository.CountSearchAsync(
            request.TicketNumber,
            request.CustomerMobile,
            request.CategoryId,
            request.PriorityId,
            status,
            request.DateFrom,
            request.DateTo,
            linkedEntityType,
            cancellationToken);

        return new PagedResult<SupportTicketListItemResponse>(
            tickets.Select(SupportTicketResponseMapper.ToListItem).ToArray(),
            totalCount,
            request.PageNumber,
            request.PageSize);
    }

    private static SupportTicketStatus? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        if (!Enum.TryParse<SupportTicketStatus>(status, true, out var parsedStatus))
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "The requested support ticket status filter is invalid.", 400);
        }

        return parsedStatus;
    }

    private static SupportTicketLinkType? ParseLinkedEntityType(string? linkedEntityType)
    {
        if (string.IsNullOrWhiteSpace(linkedEntityType))
        {
            return null;
        }

        if (!Enum.TryParse<SupportTicketLinkType>(linkedEntityType, true, out var parsedType))
        {
            throw new AppException(ErrorCodes.InvalidLookupType, "The requested linked entity type filter is invalid.", 400);
        }

        return parsedType;
    }
}
