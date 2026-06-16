using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.Admin;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.ServiceCatalogAdmin.Commands.CreateService;

public sealed record CreateServiceCommand(
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

public sealed class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
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

public sealed class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, ServiceAdminResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateServiceCommandHandler(
        IBookingLookupRepository bookingLookupRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserContext currentUserContext,
        AdminActivityLogger adminActivityLogger)
    {
        _bookingLookupRepository = bookingLookupRepository;
        _unitOfWork = unitOfWork;
        _currentUserContext = currentUserContext;
        _adminActivityLogger = adminActivityLogger;
    }

    public async Task<ServiceAdminResponse> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        _ = await _bookingLookupRepository.GetServiceCategoryForEditAsync(request.ServiceCategoryId, cancellationToken)
            ?? throw new AppException(ErrorCodes.ValidationFailure, "The selected service category does not exist.", 400);
        var pricingModel = await _bookingLookupRepository.GetPricingModelByIdAsync(request.PricingModelId, cancellationToken)
            ?? throw new AppException(ErrorCodes.ValidationFailure, "The selected pricing model does not exist.", 400);

        var service = new Service
        {
            ServiceCategoryId = request.ServiceCategoryId,
            PricingModelId = request.PricingModelId,
            ServiceName = request.ServiceName.Trim(),
            ServiceCode = string.IsNullOrWhiteSpace(request.ServiceCode)
                ? ServiceCatalogMapper.SlugCode(request.ServiceName)
                : request.ServiceCode.Trim(),
            Summary = request.Summary?.Trim() ?? string.Empty,
            BasePrice = request.BasePrice,
            EstimatedDurationInMinutes = request.EstimatedDurationInMinutes,
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            ImageAIPrompt = string.IsNullOrWhiteSpace(request.ImageAIPrompt) ? null : request.ImageAIPrompt.Trim(),
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            CreatedBy = _currentUserContext.UserName,
            IPAddress = _currentUserContext.IPAddress
        };

        _bookingLookupRepository.AddService(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _adminActivityLogger.WriteAsync(
            "CreateService",
            nameof(Service),
            service.ServiceId.ToString(),
            service.ServiceName,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        service.PricingModel = pricingModel;
        return ServiceCatalogMapper.ToServiceResponse(service);
    }
}
