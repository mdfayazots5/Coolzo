using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;

namespace Coolzo.Application.Features.Inventory;

internal static class StockTransactionTypeResolver
{
    public static StockTransactionType? ParseOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<StockTransactionType>(value.Trim(), true, out var transactionType)
            ? transactionType
            : null;
    }

    public static StockTransactionType ParseOrThrow(string value)
    {
        var transactionType = ParseOrNull(value);

        if (!transactionType.HasValue)
        {
            throw new AppException(
                ErrorCodes.InvalidStockTransactionType,
                "The supplied stock transaction type is invalid.",
                400);
        }

        return transactionType.Value;
    }
}
