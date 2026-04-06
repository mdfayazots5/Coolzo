using Coolzo.Domain.Enums;
using FluentValidation;

namespace Coolzo.Application.Features.Booking.Commands.CreateGuestBooking;

public sealed class CreateGuestBookingCommandValidator : AbstractValidator<CreateGuestBookingCommand>
{
    public CreateGuestBookingCommandValidator()
    {
        RuleFor(request => request.ServiceId).GreaterThan(0);
        RuleFor(request => request.AcTypeId).GreaterThan(0);
        RuleFor(request => request.TonnageId).GreaterThan(0);
        RuleFor(request => request.BrandId).GreaterThan(0);
        RuleFor(request => request.SlotAvailabilityId).GreaterThan(0);
        RuleFor(request => request.CustomerName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.MobileNumber).Matches("^[0-9]{8,16}$");
        RuleFor(request => request.EmailAddress).EmailAddress().When(request => !string.IsNullOrWhiteSpace(request.EmailAddress));
        RuleFor(request => request.AddressLine1).NotEmpty().MaximumLength(256);
        RuleFor(request => request.AddressLine2).MaximumLength(256);
        RuleFor(request => request.Landmark).MaximumLength(128);
        RuleFor(request => request.CityName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.Pincode).Matches("^[0-9]{4,8}$");
        RuleFor(request => request.AddressLabel).MaximumLength(64);
        RuleFor(request => request.ModelName).MaximumLength(128);
        RuleFor(request => request.IssueNotes).MaximumLength(512);
        RuleFor(request => request.IdempotencyKey).MaximumLength(128);
        RuleFor(request => request.SourceChannel)
            .NotEmpty()
            .Must(BeValidSourceChannel)
            .WithMessage("Source channel is invalid.");
    }

    private static bool BeValidSourceChannel(string sourceChannel)
    {
        return Enum.TryParse<BookingSourceChannel>(sourceChannel, true, out _);
    }
}
