using FluentValidation;

namespace Coolzo.Application.Features.Inventory.Commands.CreateWarehouse;

public sealed class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(request => request.WarehouseCode).NotEmpty().MaximumLength(32);
        RuleFor(request => request.WarehouseName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.ContactPerson).MaximumLength(128);
        RuleFor(request => request.MobileNumber).MaximumLength(32);
        RuleFor(request => request.EmailAddress).MaximumLength(128).EmailAddress().When(request => !string.IsNullOrWhiteSpace(request.EmailAddress));
        RuleFor(request => request.AddressLine1).MaximumLength(128);
        RuleFor(request => request.AddressLine2).MaximumLength(128);
        RuleFor(request => request.Landmark).MaximumLength(128);
        RuleFor(request => request.CityName).MaximumLength(64);
        RuleFor(request => request.Pincode).MaximumLength(16);
    }
}
