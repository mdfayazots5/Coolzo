using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Requests.Support;
using Coolzo.Contracts.Responses.Support;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.CreateSupportTicket;

public sealed class CreateSupportTicketCommandHandler : IRequestHandler<CreateSupportTicketCommand, SupportTicketDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<CreateSupportTicketCommandHandler> _logger;
    private readonly ISupportTicketNumberGenerator _supportTicketNumberGenerator;
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly SupportTicketAccessService _supportTicketAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSupportTicketCommandHandler(
        ISupportTicketRepository supportTicketRepository,
        ISupportTicketNumberGenerator supportTicketNumberGenerator,
        SupportTicketAccessService supportTicketAccessService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        IAppLogger<CreateSupportTicketCommandHandler> logger)
    {
        _supportTicketRepository = supportTicketRepository;
        _supportTicketNumberGenerator = supportTicketNumberGenerator;
        _supportTicketAccessService = supportTicketAccessService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<SupportTicketDetailResponse> Handle(CreateSupportTicketCommand request, CancellationToken cancellationToken)
    {
        _ = await _supportTicketRepository.GetCategoryByIdAsync(request.SupportTicketCategoryId, cancellationToken)
            ?? throw new AppException(ErrorCodes.InvalidMasterSelection, "The requested support ticket category is invalid.", 400);
        _ = await _supportTicketRepository.GetPriorityByIdAsync(request.SupportTicketPriorityId, cancellationToken)
            ?? throw new AppException(ErrorCodes.InvalidMasterSelection, "The requested support ticket priority is invalid.", 400);

        Customer? customer = null;
        long? customerId = request.CustomerId;

        if (_supportTicketAccessService.IsCustomer())
        {
            customer = await _supportTicketAccessService.GetCurrentCustomerAsync(cancellationToken);
            customerId = customer.CustomerId;
        }
        else if (customerId.HasValue)
        {
            customer = await _supportTicketRepository.GetCustomerByIdAsync(customerId.Value, cancellationToken)
                ?? throw new AppException(ErrorCodes.NotFound, "The requested customer could not be found.", 404);
        }

        var links = await BuildLinksAsync(request.Links, customerId, cancellationToken);
        customerId = links.CustomerId ?? customerId;

        if (!customerId.HasValue)
        {
            throw new AppException(
                ErrorCodes.ValidationFailure,
                "A support ticket must resolve to a valid customer.",
                400,
                new[] { (nameof(request.CustomerId), "Provide a valid customer or link the ticket to a customer-owned entity.") });
        }

        customer ??= await _supportTicketRepository.GetCustomerByIdAsync(customerId.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The resolved customer could not be found.", 404);

        var now = _currentDateTime.UtcNow;
        var userName = _currentUserContext.UserName;
        var ipAddress = _currentUserContext.IPAddress;
        var ticketNumber = await GenerateTicketNumberAsync(cancellationToken);
        var supportTicket = new SupportTicket
        {
            TicketNumber = ticketNumber,
            CustomerId = customer.CustomerId,
            SupportTicketCategoryId = request.SupportTicketCategoryId,
            SupportTicketPriorityId = request.SupportTicketPriorityId,
            CurrentStatus = SupportTicketStatus.Open,
            Subject = request.Subject.Trim(),
            Description = request.Description.Trim(),
            CreatedBy = userName,
            DateCreated = now,
            UpdatedBy = userName,
            LastUpdated = now,
            IPAddress = ipAddress
        };

        foreach (var supportTicketLink in links.Items)
        {
            supportTicket.Links.Add(supportTicketLink);
        }

        supportTicket.StatusHistories.Add(new SupportTicketStatusHistory
        {
            SupportTicketStatus = SupportTicketStatus.Open,
            Remarks = "Ticket created.",
            StatusDateUtc = now,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        });

        await _supportTicketRepository.AddTicketAsync(supportTicket, cancellationToken);
        await _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "CreateSupportTicket",
                EntityName = "SupportTicket",
                EntityId = ticketNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = request.Subject.Trim(),
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = ipAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Support ticket {SupportTicketId} created for customer {CustomerId}.", supportTicket.SupportTicketId, customer.CustomerId);

        var createdTicket = await _supportTicketAccessService.GetTicketForReadAsync(supportTicket.SupportTicketId, cancellationToken);
        var canManage = _supportTicketAccessService.CanManage();

        return SupportTicketResponseMapper.ToDetail(createdTicket, canManage, canManage, false, canManage);
    }

    private async Task<(IReadOnlyCollection<SupportTicketLink> Items, long? CustomerId)> BuildLinksAsync(
        IReadOnlyCollection<CreateSupportTicketLinkRequest> linkRequests,
        long? customerId,
        CancellationToken cancellationToken)
    {
        if (linkRequests.Count == 0)
        {
            return (Array.Empty<SupportTicketLink>(), customerId);
        }

        var now = _currentDateTime.UtcNow;
        var items = new List<SupportTicketLink>();
        var seenLinks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var linkRequest in linkRequests)
        {
            if (!Enum.TryParse<SupportTicketLinkType>(linkRequest.LinkedEntityType, true, out var linkedEntityType))
            {
                throw new AppException(ErrorCodes.InvalidLookupType, "The requested support link type is invalid.", 400);
            }

            var uniqueKey = $"{linkedEntityType}:{linkRequest.LinkedEntityId}";

            if (!seenLinks.Add(uniqueKey))
            {
                continue;
            }

            var resolution = await _supportTicketRepository.ResolveLinkedEntityAsync(linkedEntityType, linkRequest.LinkedEntityId, cancellationToken)
                ?? throw new AppException(ErrorCodes.NotFound, "One of the requested linked entities could not be found.", 404);

            if (_supportTicketAccessService.IsCustomer() && customerId.HasValue && customerId.Value != resolution.CustomerId)
            {
                throw new AppException(ErrorCodes.SupportAccessDenied, "Customers can only link tickets to their own records.", 403);
            }

            if (customerId.HasValue && customerId.Value != resolution.CustomerId)
            {
                throw new AppException(ErrorCodes.Conflict, "All linked entities must belong to the same customer.", 409);
            }

            customerId = resolution.CustomerId;
            items.Add(new SupportTicketLink
            {
                LinkedEntityType = linkedEntityType,
                LinkedEntityId = linkRequest.LinkedEntityId,
                LinkReference = resolution.LinkReference,
                LinkSummary = resolution.LinkSummary,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = now,
                IPAddress = _currentUserContext.IPAddress
            });
        }

        return (items, customerId);
    }

    private async Task<string> GenerateTicketNumberAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 20; attempt += 1)
        {
            var candidate = _supportTicketNumberGenerator.GenerateNumber();

            if (!await _supportTicketRepository.TicketNumberExistsAsync(candidate, cancellationToken))
            {
                return candidate;
            }
        }

        throw new AppException(ErrorCodes.Conflict, "Unable to allocate a unique support ticket number at this time.", 409);
    }
}
