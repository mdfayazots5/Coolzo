using FluentValidation;

namespace Coolzo.Application.Features.Support.Queries.GetSupportTicketDetail;

public sealed class GetSupportTicketDetailQueryValidator : AbstractValidator<GetSupportTicketDetailQuery>
{
    public GetSupportTicketDetailQueryValidator()
    {
        RuleFor(request => request.SupportTicketId).GreaterThan(0);
    }
}
