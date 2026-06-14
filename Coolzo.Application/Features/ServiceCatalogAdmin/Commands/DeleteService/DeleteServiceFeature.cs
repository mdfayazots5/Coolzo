using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.ServiceCatalogAdmin.Commands.DeleteService;

public sealed record DeleteServiceCommand(long ServiceId) : IRequest;

public sealed class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly IAppLogger<DeleteServiceCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteServiceCommandHandler(
        IBookingLookupRepository bookingLookupRepository,
        IUnitOfWork unitOfWork,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<DeleteServiceCommandHandler> logger)
    {
        _bookingLookupRepository = bookingLookupRepository;
        _unitOfWork = unitOfWork;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _bookingLookupRepository.GetServiceForEditAsync(request.ServiceId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service could not be found.", 404);

        _bookingLookupRepository.RemoveService(service);

        await _adminActivityLogger.WriteAsync(
            "DeleteService",
            nameof(Service),
            service.ServiceId.ToString(),
            service.ServiceName,
            cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // A hard delete fails when the service is still referenced (e.g. existing bookings).
            _logger.LogError(ex, "Hard delete blocked for ServiceId {ServiceId} — likely referenced by bookings.", service.ServiceId);
            throw new AppException(
                ErrorCodes.Conflict,
                "This service is referenced by existing bookings or records and cannot be deleted. Deactivate it instead.",
                409);
        }
    }
}
