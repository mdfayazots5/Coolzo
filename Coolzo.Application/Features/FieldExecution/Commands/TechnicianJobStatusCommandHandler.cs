using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Features.FieldExecution.Commands.MarkTechnicianJobEnRoute;
using Coolzo.Application.Features.FieldExecution.Commands.MarkTechnicianJobReached;
using Coolzo.Application.Features.FieldExecution.Commands.MarkTechnicianWorkCompleted;
using Coolzo.Application.Features.FieldExecution.Commands.MarkTechnicianWorkInProgress;
using Coolzo.Application.Features.FieldExecution.Commands.StartTechnicianWork;
using Coolzo.Application.Features.FieldExecution.Commands.SubmitTechnicianJobForClosure;
using Coolzo.Application.Features.TechnicianJob;
using Coolzo.Contracts.Responses.TechnicianJobs;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.FieldExecution.Commands;

public sealed class TechnicianJobStatusCommandHandler :
    IRequestHandler<MarkTechnicianJobEnRouteCommand, TechnicianJobDetailResponse>,
    IRequestHandler<MarkTechnicianJobReachedCommand, TechnicianJobDetailResponse>,
    IRequestHandler<StartTechnicianWorkCommand, TechnicianJobDetailResponse>,
    IRequestHandler<MarkTechnicianWorkInProgressCommand, TechnicianJobDetailResponse>,
    IRequestHandler<MarkTechnicianWorkCompletedCommand, TechnicianJobDetailResponse>,
    IRequestHandler<SubmitTechnicianJobForClosureCommand, TechnicianJobDetailResponse>
{
    private readonly ITechnicianFieldExecutionService _fieldExecutionService;
    private readonly IFieldLookupRepository _fieldLookupRepository;
    private readonly IAppLogger<TechnicianJobStatusCommandHandler> _logger;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly TechnicianJobLifecycleResolver _technicianJobLifecycleResolver;

    public TechnicianJobStatusCommandHandler(
        ITechnicianFieldExecutionService fieldExecutionService,
        IServiceRequestRepository serviceRequestRepository,
        IFieldLookupRepository fieldLookupRepository,
        ISupportTicketRepository supportTicketRepository,
        TechnicianJobLifecycleResolver technicianJobLifecycleResolver,
        IAppLogger<TechnicianJobStatusCommandHandler> logger)
    {
        _fieldExecutionService = fieldExecutionService;
        _serviceRequestRepository = serviceRequestRepository;
        _fieldLookupRepository = fieldLookupRepository;
        _supportTicketRepository = supportTicketRepository;
        _technicianJobLifecycleResolver = technicianJobLifecycleResolver;
        _logger = logger;
    }

    public Task<TechnicianJobDetailResponse> Handle(MarkTechnicianJobEnRouteCommand request, CancellationToken cancellationToken)
    {
        return AdvanceAsync(
            request.ServiceRequestId,
            ServiceRequestStatus.EnRoute,
            request.Remarks,
            request.WorkSummary,
            "MarkTechnicianJobEnRoute",
            cancellationToken);
    }

    public Task<TechnicianJobDetailResponse> Handle(MarkTechnicianJobReachedCommand request, CancellationToken cancellationToken)
    {
        return AdvanceAsync(
            request.ServiceRequestId,
            ServiceRequestStatus.Reached,
            request.Remarks,
            request.WorkSummary,
            "MarkTechnicianJobReached",
            cancellationToken);
    }

    public Task<TechnicianJobDetailResponse> Handle(StartTechnicianWorkCommand request, CancellationToken cancellationToken)
    {
        return AdvanceAsync(
            request.ServiceRequestId,
            ServiceRequestStatus.WorkStarted,
            request.Remarks,
            request.WorkSummary,
            "StartTechnicianWork",
            cancellationToken);
    }

    public Task<TechnicianJobDetailResponse> Handle(MarkTechnicianWorkInProgressCommand request, CancellationToken cancellationToken)
    {
        return AdvanceAsync(
            request.ServiceRequestId,
            ServiceRequestStatus.WorkInProgress,
            request.Remarks,
            request.WorkSummary,
            "MarkTechnicianWorkInProgress",
            cancellationToken);
    }

    public Task<TechnicianJobDetailResponse> Handle(MarkTechnicianWorkCompletedCommand request, CancellationToken cancellationToken)
    {
        return AdvanceAsync(
            request.ServiceRequestId,
            ServiceRequestStatus.WorkCompletedPendingSubmission,
            request.Remarks,
            request.WorkSummary,
            "MarkTechnicianWorkCompleted",
            cancellationToken);
    }

    public Task<TechnicianJobDetailResponse> Handle(SubmitTechnicianJobForClosureCommand request, CancellationToken cancellationToken)
    {
        return AdvanceAsync(
            request.ServiceRequestId,
            ServiceRequestStatus.SubmittedForClosure,
            request.Remarks,
            request.WorkSummary,
            "SubmitTechnicianJobForClosure",
            cancellationToken);
    }

    private async Task<TechnicianJobDetailResponse> AdvanceAsync(
        long serviceRequestId,
        ServiceRequestStatus targetStatus,
        string? remarks,
        string? workSummary,
        string auditActionName,
        CancellationToken cancellationToken)
    {
        await _fieldExecutionService.AdvanceStatusAsync(
            serviceRequestId,
            targetStatus,
            remarks,
            workSummary,
            auditActionName,
            cancellationToken);

        _logger.LogInformation(
            "Technician job {ServiceRequestId} advanced to {Status}.",
            serviceRequestId,
            targetStatus);

        var refreshedServiceRequest = await _serviceRequestRepository.GetByIdAsync(serviceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The updated technician job could not be loaded.", 404);
        var serviceId = refreshedServiceRequest.Booking?.BookingLines
            .OrderBy(line => line.BookingLineId)
            .Select(line => line.ServiceId)
            .FirstOrDefault() ?? 0;
        var checklistMasters = serviceId > 0
            ? await _fieldLookupRepository.GetChecklistByServiceIdAsync(serviceId, cancellationToken)
            : Array.Empty<Coolzo.Domain.Entities.ServiceChecklistMaster>();
        var (lifecycleType, lifecycleLabel) = await _technicianJobLifecycleResolver.ResolveAsync(serviceRequestId, cancellationToken);
        var supportJobAlert = await _supportTicketRepository.GetJobAlertAsync(
            refreshedServiceRequest.ServiceRequestId,
            refreshedServiceRequest.BookingId,
            refreshedServiceRequest.JobCard?.JobCardId,
            cancellationToken);

        return TechnicianJobResponseMapper.ToDetail(refreshedServiceRequest, checklistMasters, lifecycleType, lifecycleLabel, supportJobAlert);
    }
}
