using FluentValidation;

namespace Coolzo.Application.Features.Support.Queries.GetSupportTicketHistory;

public sealed class GetSupportTicketHistoryQueryValidator : AbstractValidator<GetSupportTicketHistoryQuery>
{
    public GetSupportTicketHistoryQueryValidator()
    {
        RuleFor(request => request.SupportTicketId).GreaterThan(0);
    }
}
