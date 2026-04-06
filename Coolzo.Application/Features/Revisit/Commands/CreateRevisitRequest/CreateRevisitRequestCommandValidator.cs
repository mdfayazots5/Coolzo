using Coolzo.Domain.Enums;
using FluentValidation;

namespace Coolzo.Application.Features.Revisit.Commands.CreateRevisitRequest;

public sealed class CreateRevisitRequestCommandValidator : AbstractValidator<CreateRevisitRequestCommand>
{
    public CreateRevisitRequestCommandValidator()
    {
        RuleFor(request => request.OriginalJobCardId).GreaterThan(0);
        RuleFor(request => request.RevisitType)
            .NotEmpty()
            .Must(value => Enum.TryParse<RevisitType>(value, true, out _))
            .WithMessage("Revisit type is invalid.");
        RuleFor(request => request.IssueSummary).NotEmpty().MaximumLength(256);
        RuleFor(request => request.RequestRemarks).MaximumLength(512);
        RuleFor(request => request.ChargeAmount)
            .GreaterThanOrEqualTo(0.00m)
            .When(request => request.ChargeAmount.HasValue);
    }
}
