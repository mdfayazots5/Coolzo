using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Contracts.Responses.Admin;
using Coolzo.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.ServiceCatalogAdmin.Commands.CreateServiceCategory;

public sealed record CreateServiceCategoryCommand(
    string CategoryName,
    string? CategoryCode,
    string? Description,
    string? ImageUrl,
    string? ImageAIPrompt,
    bool IsActive,
    int SortOrder) : IRequest<ServiceCategoryAdminResponse>;

public sealed class CreateServiceCategoryCommandValidator : AbstractValidator<CreateServiceCategoryCommand>
{
    public CreateServiceCategoryCommandValidator()
    {
        RuleFor(request => request.CategoryName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.CategoryCode).MaximumLength(64);
        RuleFor(request => request.Description).MaximumLength(512);
        RuleFor(request => request.ImageUrl).MaximumLength(512);
        RuleFor(request => request.ImageAIPrompt).MaximumLength(1024);
    }
}

public sealed class CreateServiceCategoryCommandHandler
    : IRequestHandler<CreateServiceCategoryCommand, ServiceCategoryAdminResponse>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateServiceCategoryCommandHandler(
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

    public async Task<ServiceCategoryAdminResponse> Handle(CreateServiceCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new ServiceCategory
        {
            CategoryName = request.CategoryName.Trim(),
            CategoryCode = string.IsNullOrWhiteSpace(request.CategoryCode)
                ? ServiceCatalogMapper.SlugCode(request.CategoryName)
                : request.CategoryCode.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            ImageAIPrompt = string.IsNullOrWhiteSpace(request.ImageAIPrompt) ? null : request.ImageAIPrompt.Trim(),
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            CreatedBy = _currentUserContext.UserName,
            IPAddress = _currentUserContext.IPAddress
        };

        _bookingLookupRepository.AddServiceCategory(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _adminActivityLogger.WriteAsync(
            "CreateServiceCategory",
            nameof(ServiceCategory),
            category.ServiceCategoryId.ToString(),
            category.CategoryName,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceCatalogMapper.ToCategoryResponse(category, 0);
    }
}
