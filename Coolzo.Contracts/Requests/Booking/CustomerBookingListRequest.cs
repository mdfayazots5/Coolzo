namespace Coolzo.Contracts.Requests.Booking;

public sealed record CustomerBookingListRequest(int PageNumber = 1, int PageSize = 20);
