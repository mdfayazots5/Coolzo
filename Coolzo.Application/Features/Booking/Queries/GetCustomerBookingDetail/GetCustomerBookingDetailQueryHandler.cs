using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Booking;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Booking.Queries.GetCustomerBookingDetail;

public sealed class GetCustomerBookingDetailQueryHandler : IRequestHandler<GetCustomerBookingDetailQuery, BookingDetailResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IFieldLookupRepository _fieldLookupRepository;

    public GetCustomerBookingDetailQueryHandler(
        IBookingRepository bookingRepository,
        ICurrentUserContext currentUserContext,
        IFieldLookupRepository fieldLookupRepository)
    {
        _bookingRepository = bookingRepository;
        _currentUserContext = currentUserContext;
        _fieldLookupRepository = fieldLookupRepository;
    }

    public async Task<BookingDetailResponse> Handle(GetCustomerBookingDetailQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
        {
            throw new AppException(ErrorCodes.Unauthorized, "Customer authentication is required.", 401);
        }

        var customer = await _bookingRepository.GetCustomerByUserIdAsync(_currentUserContext.UserId.Value, cancellationToken)
            ?? throw new AppException(ErrorCodes.BookingAccessDenied, "This booking does not belong to the current customer.", 403);

        var booking = await _bookingRepository.GetByIdForCustomerAsync(request.BookingId, customer.CustomerId, cancellationToken)
            ?? throw new AppException(ErrorCodes.BookingAccessDenied, "This booking does not belong to the current customer.", 403);
        var serviceId = booking.BookingLines
            .OrderBy(line => line.BookingLineId)
            .Select(line => line.ServiceId)
            .FirstOrDefault();
        var checklistMasters = serviceId > 0
            ? await _fieldLookupRepository.GetChecklistByServiceIdAsync(serviceId, cancellationToken)
            : Array.Empty<Coolzo.Domain.Entities.ServiceChecklistMaster>();

        return BookingResponseMapper.ToDetail(booking, checklistMasters);
    }
}
