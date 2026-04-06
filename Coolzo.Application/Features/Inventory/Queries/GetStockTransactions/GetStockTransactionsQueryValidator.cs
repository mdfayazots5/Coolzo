using FluentValidation;

namespace Coolzo.Application.Features.Inventory.Queries.GetStockTransactions;

public sealed class GetStockTransactionsQueryValidator : AbstractValidator<GetStockTransactionsQuery>
{
    public GetStockTransactionsQueryValidator()
    {
        RuleFor(request => request.TransactionType).MaximumLength(64);
        RuleFor(request => request.PageNumber).GreaterThan(0);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 200);
    }
}
