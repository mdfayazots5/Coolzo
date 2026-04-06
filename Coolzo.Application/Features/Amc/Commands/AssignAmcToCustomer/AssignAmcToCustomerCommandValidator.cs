using FluentValidation;

namespace Coolzo.Application.Features.Amc.Commands.AssignAmcToCustomer;

public sealed class AssignAmcToCustomerCommandValidator : AbstractValidator<AssignAmcToCustomerCommand>
{
    public AssignAmcToCustomerCommandValidator()
    {
        RuleFor(request => request.CustomerId).GreaterThan(0);
        RuleFor(request => request.AmcPlanId).GreaterThan(0);
        RuleFor(request => request.JobCardId).GreaterThan(0);
        RuleFor(request => request.InvoiceId).GreaterThan(0);
        RuleFor(request => request.Remarks).MaximumLength(512);
    }
}
