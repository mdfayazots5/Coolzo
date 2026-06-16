using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.Booking;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.ServiceCatalogAdmin.Commands.SetServicePrompt;

public sealed record SetServicePromptCommand(long ServiceId, string? ImageAIPrompt)
    : IRequest<ServiceLookupResponse>;

public sealed class SetServicePromptCommandValidator : AbstractValidator<SetServicePromptCommand>
{
    public SetServicePromptCommandValidator()
    {
        RuleFor(request => request.ServiceId).GreaterThan(0);
        RuleFor(request => request.ImageAIPrompt).MaximumLength(1024);
    }
}

public sealed class SetServicePromptCommandHandler : IRequestHandler<SetServicePromptCommand, ServiceLookupResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAppLogger<SetServicePromptCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public SetServicePromptCommandHandler(
        IBookingLookupRepository bookingLookupRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger,
        IAppLogger<SetServicePromptCommandHandler> logger)
    {
        _bookingLookupRepository = bookingLookupRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
        _logger = logger;
    }

    public async Task<ServiceLookupResponse> Handle(SetServicePromptCommand request, CancellationToken cancellationToken)
    {
        var service = await _bookingLookupRepository.GetServiceByIdAsync(request.ServiceId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service could not be found.", 404);

        var prompt = string.IsNullOrWhiteSpace(request.ImageAIPrompt) ? null : request.ImageAIPrompt.Trim();
        service.ImageAIPrompt = prompt;
        service.UpdatedBy = _currentUserContext.UserName;
        service.LastUpdated = _currentDateTime.UtcNow;

        await _adminActivityLogger.WriteAsync(
            "SetServicePrompt",
            nameof(Coolzo.Domain.Entities.Service),
            service.ServiceId.ToString(),
            prompt is null ? "(cleared)" : "(updated)",
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Service AI image prompt {Action} for ServiceId {ServiceId} by {UserName}.",
            prompt is null ? "cleared" : "set",
            service.ServiceId,
            _currentUserContext.UserName);

        return new ServiceLookupResponse(
            service.ServiceId,
            service.ServiceCategoryId,
            service.ServiceName,
            service.Summary,
            service.BasePrice,
            service.PricingModel?.PricingModelName ?? string.Empty,
            service.ImageUrl ?? string.Empty,
            service.ImageAIPrompt ?? string.Empty);
    }
}
