using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.BookingLookup.Queries.GetZoneByPincode;

public sealed record GetZoneByPincodeQuery(string Pincode) : IRequest<ZoneLookupResponse>;
