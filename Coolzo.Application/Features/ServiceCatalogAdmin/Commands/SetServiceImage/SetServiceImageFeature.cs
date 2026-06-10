using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.Booking;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.ServiceCatalogAdmin.Commands.SetServiceImage;

public sealed record SetServiceImageCommand(long ServiceId, string? ImageUrl)
    : IRequest<ServiceLookupResponse>;

public sealed class SetServiceImageCommandValidator : AbstractValidator<SetServiceImageCommand>
{
    public SetServiceImageCommandValidator()
    {
        RuleFor(request => request.ServiceId).GreaterThan(0);
        RuleFor(request => request.ImageUrl).MaximumLength(512);
    }
}

public sealed class SetServiceImageCommandHandler : IRequestHandler<SetServiceImageCommand, ServiceLookupResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<SetServiceImageCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public SetServiceImageCommandHandler(
        IBookingLookupRepository bookingLookupRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<SetServiceImageCommandHandler> logger)
    {
        _bookingLookupRepository = bookingLookupRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<ServiceLookupResponse> Handle(SetServiceImageCommand request, CancellationToken cancellationToken)
    {
        var service = await _bookingLookupRepository.GetServiceByIdAsync(request.ServiceId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service could not be found.", 404);

        var imageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
        service.ImageUrl = imageUrl;
        service.UpdatedBy = _currentUserContext.UserName;
        service.LastUpdated = _currentDateTime.UtcNow;

        await _adminActivityLogger.WriteAsync(
            "SetServiceImage",
            nameof(Coolzo.Domain.Entities.Service),
            service.ServiceId.ToString(),
            imageUrl ?? "(cleared)",
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Service image {Action} for ServiceId {ServiceId} by {UserName}.",
            imageUrl is null ? "cleared" : "set",
            service.ServiceId,
            _currentUserContext.UserName);

        return new ServiceLookupResponse(
            service.ServiceId,
            service.ServiceCategoryId,
            service.ServiceName,
            service.Summary,
            service.BasePrice,
            service.PricingModel?.PricingModelName ?? string.Empty,
            service.ImageUrl ?? string.Empty);
    }
}
