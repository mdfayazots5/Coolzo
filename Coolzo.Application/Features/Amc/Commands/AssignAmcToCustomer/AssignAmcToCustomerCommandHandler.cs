using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Amc;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Amc.Commands.AssignAmcToCustomer;

public sealed class AssignAmcToCustomerCommandHandler : IRequestHandler<AssignAmcToCustomerCommand, CustomerAmcResponse>
{
    private readonly IAmcRepository _amcRepository;
    private readonly AmcScheduleService _amcScheduleService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<AssignAmcToCustomerCommandHandler> _logger;
    private readonly ServiceLifecycleAccessService _serviceLifecycleAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public AssignAmcToCustomerCommandHandler(
        IAmcRepository amcRepository,
        AmcScheduleService amcScheduleService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        ServiceLifecycleAccessService serviceLifecycleAccessService,
        IAppLogger<AssignAmcToCustomerCommandHandler> logger)
    {
        _amcRepository = amcRepository;
        _amcScheduleService = amcScheduleService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _serviceLifecycleAccessService = serviceLifecycleAccessService;
        _logger = logger;
    }

    public async Task<CustomerAmcResponse> Handle(AssignAmcToCustomerCommand request, CancellationToken cancellationToken)
    {
        _serviceLifecycleAccessService.EnsureAmcAssignmentAccess();

        var customer = await _amcRepository.GetCustomerByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested customer could not be found.", 404);
        var amcPlan = await _amcRepository.GetAmcPlanByIdAsync(request.AmcPlanId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested AMC plan could not be found.", 404);
        var jobCard = await _amcRepository.GetJobCardByIdAsync(request.JobCardId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested job card could not be found.", 404);
        var invoice = await _amcRepository.GetInvoiceByIdAsync(request.InvoiceId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested invoice could not be found.", 404);

        if (!amcPlan.IsActive)
        {
            throw new AppException(ErrorCodes.ValidationFailure, "Only active AMC plans can be assigned.", 400);
        }

        if (invoice.CustomerId != customer.CustomerId)
        {
            throw new AppException(ErrorCodes.ValidationFailure, "The invoice does not belong to the requested customer.", 400);
        }

        if (invoice.QuotationHeader?.JobCardId != jobCard.JobCardId)
        {
            throw new AppException(ErrorCodes.ValidationFailure, "The invoice does not belong to the selected job card.", 400);
        }

        if (jobCard.WorkCompletedDateUtc is null && jobCard.SubmittedForClosureDateUtc is null)
        {
            throw new AppException(
                ErrorCodes.InvalidStatusTransition,
                "AMC assignment is allowed only after the originating job has been completed.",
                409);
        }

        var startDateUtc = request.StartDateUtc ?? invoice.InvoiceDateUtc;
        var existingSubscriptions = await _amcRepository.GetCustomerAmcByCustomerIdAsync(customer.CustomerId, cancellationToken);

        if (existingSubscriptions.Any(entity => entity.InvoiceHeaderId == request.InvoiceId))
        {
            throw new AppException(ErrorCodes.CustomerAmcAlreadyExists, "This invoice already has an AMC subscription linked to it.", 409);
        }

        if (await _amcRepository.HasActiveCustomerAmcAsync(customer.CustomerId, amcPlan.AmcPlanId, startDateUtc, cancellationToken))
        {
            throw new AppException(
                ErrorCodes.CustomerAmcAlreadyExists,
                "An active AMC subscription already exists for the customer and plan during the selected coverage period.",
                409);
        }

        var customerAmc = new CustomerAmc
        {
            CustomerId = customer.CustomerId,
            AmcPlanId = amcPlan.AmcPlanId,
            JobCardId = jobCard.JobCardId,
            InvoiceHeaderId = invoice.InvoiceHeaderId,
            CurrentStatus = AmcSubscriptionStatus.Active,
            StartDateUtc = startDateUtc,
            EndDateUtc = startDateUtc.Date.AddMonths(amcPlan.DurationInMonths).AddDays(-1),
            TotalVisitCount = amcPlan.VisitCount,
            ConsumedVisitCount = 0,
            PriceAmount = amcPlan.PriceAmount,
            Comments = request.Remarks?.Trim(),
            CreatedBy = _currentUserContext.UserName,
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = _currentUserContext.IPAddress
        };

        foreach (var schedule in _amcScheduleService.BuildInitialSchedule(
            customerAmc,
            _currentUserContext.UserName,
            _currentUserContext.IPAddress,
            _currentDateTime.UtcNow))
        {
            customerAmc.Visits.Add(schedule);
        }

        await _amcRepository.AddCustomerAmcAsync(customerAmc, cancellationToken);
        await AddAuditLogAsync(invoice.InvoiceNumber, customerAmc.CurrentStatus.ToString(), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var savedSubscription = await _amcRepository.GetCustomerAmcByIdAsync(customerAmc.CustomerAmcId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The saved AMC subscription could not be reloaded.", 404);

        _logger.LogInformation(
            "AMC plan {PlanName} assigned to customer {CustomerId} against invoice {InvoiceNumber}.",
            amcPlan.PlanName,
            customer.CustomerId,
            invoice.InvoiceNumber);

        return AmcResponseMapper.ToCustomerAmc(savedSubscription);
    }

    private Task AddAuditLogAsync(string invoiceNumber, string statusName, CancellationToken cancellationToken)
    {
        return _auditLogRepository.AddAsync(
            new AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "AssignAmcToCustomer",
                EntityName = "CustomerAmc",
                EntityId = invoiceNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = statusName,
                NewValues = invoiceNumber,
                CreatedBy = _currentUserContext.UserName,
                DateCreated = _currentDateTime.UtcNow,
                IPAddress = _currentUserContext.IPAddress
            },
            cancellationToken);
    }
}
