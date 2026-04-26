using System.Text.RegularExpressions;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;

namespace Coolzo.Application.Features.NotificationTemplate;

internal static partial class NotificationTemplateMergeTagCatalog
{
    private static readonly HashSet<string> AllowedTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        "Customer.FullName",
        "Customer.FirstName",
        "CustomerName",
        "Booking.Reference",
        "BookingReference",
        "Booking.SlotDate",
        "VisitDate",
        "Booking.SlotLabel",
        "Invoice.Number",
        "Invoice.Amount",
        "ServiceName",
        "Support.TicketNumber"
    };

    public static IReadOnlyCollection<string> Normalize(IReadOnlyCollection<string> source)
    {
        return source
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static void ValidateAllowedTags(IReadOnlyCollection<string> source)
    {
        var invalidTokens = Normalize(source)
            .Where(item => !AllowedTokens.Contains(item))
            .ToArray();

        if (invalidTokens.Length != 0)
        {
            throw new AppException(ErrorCodes.ValidationFailure, $"Unsupported merge tags: {string.Join(", ", invalidTokens)}.", 400);
        }
    }

    public static void ValidateTemplateTokens(string? subjectTemplate, string bodyTemplate)
    {
        var discoveredTokens = ExtractTokens(subjectTemplate, bodyTemplate)
            .Where(item => !AllowedTokens.Contains(item))
            .ToArray();

        if (discoveredTokens.Length != 0)
        {
            throw new AppException(ErrorCodes.ValidationFailure, $"The template contains unsupported merge tags: {string.Join(", ", discoveredTokens)}.", 400);
        }
    }

    [GeneratedRegex(@"\{\{(?<token>[A-Za-z0-9_.]+)\}\}")]
    private static partial Regex MergeTokenRegex();

    private static IReadOnlyCollection<string> ExtractTokens(params string?[] content)
    {
        var values = new List<string>();

        foreach (var item in content.Where(item => !string.IsNullOrWhiteSpace(item)))
        {
            values.AddRange(
                MergeTokenRegex()
                    .Matches(item!)
                    .Select(match => match.Groups["token"].Value));
        }

        return Normalize(values);
    }
}
