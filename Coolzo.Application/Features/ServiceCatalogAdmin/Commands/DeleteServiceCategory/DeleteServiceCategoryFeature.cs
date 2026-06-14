using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Services;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.ServiceCatalogAdmin.Commands.DeleteServiceCategory;

public sealed record DeleteServiceCategoryCommand(long ServiceCategoryId) : IRequest;

public sealed class DeleteServiceCategoryCommandHandler : IRequestHandler<DeleteServiceCategoryCommand>
{
    private readonly AdminActivityLogger _adminActivityLogger;
    private readonly IBookingLookupRepository _bookingLookupRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteServiceCategoryCommandHandler(
        IBookingLookupRepository bookingLookupRepository,
        IUnitOfWork unitOfWork,
        AdminActivityLogger adminActivityLogger)
    {
        _bookingLookupRepository = bookingLookupRepository;
        _unitOfWork = unitOfWork;
        _adminActivityLogger = adminActivityLogger;
    }

    public async Task Handle(DeleteServiceCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _bookingLookupRepository.GetServiceCategoryForEditAsync(request.ServiceCategoryId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested service category could not be found.", 404);

        // Hard delete is guarded: a category that still owns services cannot be removed (would orphan
        // services / violate the FK). The admin must move or delete its services first.
        var serviceCount = await _bookingLookupRepository.CountServicesInCategoryAsync(category.ServiceCategoryId, cancellationToken);
        if (serviceCount > 0)
        {
            throw new AppException(
                ErrorCodes.Conflict,
                $"This category still has {serviceCount} service(s). Remove or reassign them before deleting the category.",
                409);
        }

        _bookingLookupRepository.RemoveServiceCategory(category);

        await _adminActivityLogger.WriteAsync(
            "DeleteServiceCategory",
            nameof(ServiceCategory),
            category.ServiceCategoryId.ToString(),
            category.CategoryName,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
