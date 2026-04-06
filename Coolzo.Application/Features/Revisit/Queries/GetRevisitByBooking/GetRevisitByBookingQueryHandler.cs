using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Features.Amc;
using Coolzo.Contracts.Responses.Revisit;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Revisit.Queries.GetRevisitByBooking;

public sealed class GetRevisitByBookingQueryHandler : IRequestHandler<GetRevisitByBookingQuery, IReadOnlyCollection<RevisitRequestResponse>>
{
    private readonly IAmcRepository _amcRepository;
    private readonly ServiceLifecycleAccessService _serviceLifecycleAccessService;

    public GetRevisitByBookingQueryHandler(
        IAmcRepository amcRepository,
        ServiceLifecycleAccessService serviceLifecycleAccessService)
    {
        _amcRepository = amcRepository;
        _serviceLifecycleAccessService = serviceLifecycleAccessService;
    }

    public async Task<IReadOnlyCollection<RevisitRequestResponse>> Handle(GetRevisitByBookingQuery request, CancellationToken cancellationToken)
    {
        var booking = await _amcRepository.GetBookingByIdAsync(request.BookingId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested booking could not be found.", 404);

        await _serviceLifecycleAccessService.EnsureBookingReadAccessAsync(booking, cancellationToken);

        var revisitRequests = await _amcRepository.GetRevisitRequestsByBookingIdAsync(request.BookingId, cancellationToken);

        return revisitRequests
            .Select(RevisitResponseMapper.ToResponse)
            .ToArray();
    }
}
