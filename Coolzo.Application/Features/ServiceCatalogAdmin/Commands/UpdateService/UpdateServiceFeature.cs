using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.Admin;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.ServiceCatalogAdmin.Commands.UpdateService;

public sealed record UpdateServiceCommand(
    long ServiceId,
    long ServiceCategoryId,
    long PricingModelId,
    string ServiceName,
    string? ServiceCode,
    string? Summary,
    decimal BasePrice,
    int EstimatedDurationInMinutes,
    string? ImageUrl,
    string? ImageAIPrompt,
    bool IsActive,
    int SortOrder) : IRequest<ServiceAdminResponse>;

public sealed class UpdateServiceCommandValidator : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceCommandValidator()
    {
        RuleFor(request => request.ServiceId).GreaterThan(0);
        RuleFor(request => request.ServiceCategoryId).GreaterThan(0);
        RuleFor(request => request.PricingModelId).GreaterThan(0);
        RuleFor(request => request.ServiceName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.ServiceCode).MaximumLength(64);
        RuleFor(request => request.Summary).MaximumLength(512);
        RuleFor(request => request.BasePrice).GreaterThanOrEqualTo(0);
        RuleFor(request => request.EstimatedDurationInMinutes).GreaterThanOrEqualTo(0);
        RuleFor(request => request.ImageUrl).MaximumLength(512);
        RuleFor(request => request.ImageAIPrompt).MaximumLength(1024);
    }
}

public sealed class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, ServiceAdminResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceCommandHandler(
        IBookingLookupRepository bookingLookupRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger)
    {
        _bookingLookupRepository = bookingLookupRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
    }

    public async Task<ServiceAdminResponse> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _bookingLookupRepository.GetServiceForEditAsync(request.ServiceId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service could not be found.", 404);

        _ = await _bookingLookupRepository.GetServiceCategoryForEditAsync(request.ServiceCategoryId, cancellationToken)
            ?? throw new AppException(ErrorCodes.ValidationFailure, "The selected service category does not exist.", 400);
        var pricingModel = await _bookingLookupRepository.GetPricingModelByIdAsync(request.PricingModelId, cancellationToken)
            ?? throw new AppException(ErrorCodes.ValidationFailure, "The selected pricing model does not exist.", 400);

        service.ServiceCategoryId = request.ServiceCategoryId;
        service.PricingModelId = request.PricingModelId;
        service.ServiceName = request.ServiceName.Trim();
        service.ServiceCode = string.IsNullOrWhiteSpace(request.ServiceCode)
            ? ServiceCatalogMapper.SlugCode(request.ServiceName)
            : request.ServiceCode.Trim();
        service.Summary = request.Summary?.Trim() ?? string.Empty;
        service.BasePrice = request.BasePrice;
        service.EstimatedDurationInMinutes = request.EstimatedDurationInMinutes;
        service.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
        service.ImageAIPrompt = string.IsNullOrWhiteSpace(request.ImageAIPrompt) ? null : request.ImageAIPrompt.Trim();
        service.IsActive = request.IsActive;
        service.SortOrder = request.SortOrder;
        service.UpdatedBy = _currentUserContext.UserName;
        service.LastUpdated = _currentDateTime.UtcNow;

        await _adminActivityLogger.WriteAsync(
            "UpdateService",
            nameof(Service),
            service.ServiceId.ToString(),
            service.ServiceName,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        service.PricingModel = pricingModel;
        return ServiceCatalogMapper.ToServiceResponse(service);
    }
}
