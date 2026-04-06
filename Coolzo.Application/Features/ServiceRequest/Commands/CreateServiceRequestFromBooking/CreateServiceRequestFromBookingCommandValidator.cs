using FluentValidation;

namespace Coolzo.Application.Features.ServiceRequest.Commands.CreateServiceRequestFromBooking;

public sealed class CreateServiceRequestFromBookingCommandValidator : AbstractValidator<CreateServiceRequestFromBookingCommand>
{
    public CreateServiceRequestFromBookingCommandValidator()
    {
        RuleFor(request => request.BookingId).GreaterThan(0);
    }
}
