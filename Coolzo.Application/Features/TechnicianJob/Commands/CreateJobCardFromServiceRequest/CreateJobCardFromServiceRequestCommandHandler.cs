using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Features.TechnicianJob;
using Coolzo.Contracts.Responses.FieldExecution;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.TechnicianJob.Commands.CreateJobCardFromServiceRequest;

public sealed class CreateJobCardFromServiceRequestCommandHandler : IRequestHandler<CreateJobCardFromServiceRequestCommand, JobCardSummaryResponse>
{
    private readonly IJobCardFactory _jobCardFactory;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateJobCardFromServiceRequestCommandHandler(
        IServiceRequestRepository serviceRequestRepository,
        IJobCardFactory jobCardFactory,
        IUnitOfWork unitOfWork)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _jobCardFactory = jobCardFactory;
        _unitOfWork = unitOfWork;
    }

    public async Task<JobCardSummaryResponse> Handle(CreateJobCardFromServiceRequestCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _serviceRequestRepository.GetByIdForUpdateAsync(request.ServiceRequestId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service request could not be found.", 404);

        var jobCard = _jobCardFactory.EnsureCreated(serviceRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TechnicianJobResponseMapper.ToJobCardSummary(jobCard);
    }
}
