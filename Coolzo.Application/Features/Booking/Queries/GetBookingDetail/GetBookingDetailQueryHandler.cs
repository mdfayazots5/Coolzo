using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Booking;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.Booking.Queries.GetBookingDetail;

public sealed class GetBookingDetailQueryHandler : IRequestHandler<GetBookingDetailQuery, BookingDetailResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IFieldLookupRepository _fieldLookupRepository;

    public GetBookingDetailQueryHandler(
        IBookingRepository bookingRepository,
        IFieldLookupRepository fieldLookupRepository)
    {
        _bookingRepository = bookingRepository;
        _fieldLookupRepository = fieldLookupRepository;
    }

    public async Task<BookingDetailResponse> Handle(GetBookingDetailQuery request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested booking could not be found.", 404);
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
