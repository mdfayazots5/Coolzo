using FluentValidation;

namespace Coolzo.Application.Features.Support.Queries.GetSupportTicketReplies;

public sealed class GetSupportTicketRepliesQueryValidator : AbstractValidator<GetSupportTicketRepliesQuery>
{
    public GetSupportTicketRepliesQueryValidator()
    {
        RuleFor(request => request.SupportTicketId).GreaterThan(0);
    }
}
