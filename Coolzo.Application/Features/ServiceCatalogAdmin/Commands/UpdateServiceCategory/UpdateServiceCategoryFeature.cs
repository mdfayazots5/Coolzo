using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.Admin;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.ServiceCatalogAdmin.Commands.UpdateServiceCategory;

public sealed record UpdateServiceCategoryCommand(
    long ServiceCategoryId,
    string CategoryName,
    string? CategoryCode,
    string? Description,
    string? ImageUrl,
    bool IsActive,
    int SortOrder) : IRequest<ServiceCategoryAdminResponse>;

public sealed class UpdateServiceCategoryCommandValidator : AbstractValidator<UpdateServiceCategoryCommand>
{
    public UpdateServiceCategoryCommandValidator()
    {
        RuleFor(request => request.ServiceCategoryId).GreaterThan(0);
        RuleFor(request => request.CategoryName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.CategoryCode).MaximumLength(64);
        RuleFor(request => request.Description).MaximumLength(512);
        RuleFor(request => request.ImageUrl).MaximumLength(512);
    }
}

public sealed class UpdateServiceCategoryCommandHandler
    : IRequestHandler<UpdateServiceCategoryCommand, ServiceCategoryAdminResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceCategoryCommandHandler(
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

    public async Task<ServiceCategoryAdminResponse> Handle(UpdateServiceCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _bookingLookupRepository.GetServiceCategoryForEditAsync(request.ServiceCategoryId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service category could not be found.", 404);

        category.CategoryName = request.CategoryName.Trim();
        category.CategoryCode = string.IsNullOrWhiteSpace(request.CategoryCode)
            ? ServiceCatalogMapper.SlugCode(request.CategoryName)
            : request.CategoryCode.Trim();
        category.Description = request.Description?.Trim() ?? string.Empty;
        category.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
        category.IsActive = request.IsActive;
        category.SortOrder = request.SortOrder;
        category.UpdatedBy = _currentUserContext.UserName;
        category.LastUpdated = _currentDateTime.UtcNow;

        await _adminActivityLogger.WriteAsync(
            "UpdateServiceCategory",
            nameof(ServiceCategory),
            category.ServiceCategoryId.ToString(),
            category.CategoryName,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var serviceCount = await _bookingLookupRepository.CountServicesInCategoryAsync(category.ServiceCategoryId, cancellationToken);
        return ServiceCatalogMapper.ToCategoryResponse(category, serviceCount);
    }
}
