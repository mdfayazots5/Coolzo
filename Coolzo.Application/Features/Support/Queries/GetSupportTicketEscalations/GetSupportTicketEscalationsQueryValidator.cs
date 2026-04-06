using FluentValidation;

namespace Coolzo.Application.Features.Support.Queries.GetSupportTicketEscalations;

public sealed class GetSupportTicketEscalationsQueryValidator : AbstractValidator<GetSupportTicketEscalationsQuery>
{
    public GetSupportTicketEscalationsQueryValidator()
    {
        RuleFor(request => request.SupportTicketId).GreaterThan(0);
    }
}
