using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Billing;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Billing.Queries.GetAccountsReceivableDashboard;

public sealed record GetAccountsReceivableDashboardQuery() : IRequest<AccountsReceivableDashboardResponse>;

public sealed class GetAccountsReceivableDashboardQueryHandler
    : IRequestHandler<GetAccountsReceivableDashboardQuery, AccountsReceivableDashboardResponse>
{
    private static readonly (string Label, string Color, int MinDays, int? MaxDays)[] AgingBands =
    [
        ("0-30 Days", "bg-status-completed", int.MinValue, 30),
        ("31-60 Days", "bg-brand-gold", 31, 60),
        ("61-90 Days", "bg-status-pending", 61, 90),
        ("90+ Days", "bg-status-emergency", 91, null),
    ];

    private readonly IBillingRepository _billingRepository;
    private readonly ICurrentDateTime _currentDateTime;

    public GetAccountsReceivableDashboardQueryHandler(
        IBillingRepository billingRepository,
        ICurrentDateTime currentDateTime)
    {
        _billingRepository = billingRepository;
        _currentDateTime = currentDateTime;
    }

    public async Task<AccountsReceivableDashboardResponse> Handle(
        GetAccountsReceivableDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var invoices = await _billingRepository.ListAccountsReceivableInvoicesAsync(cancellationToken);
        var now = _currentDateTime.UtcNow;

        var overdueInvoices = invoices
            .Where(invoice => invoice.BalanceAmount > 0)
            .Select(invoice => new
            {
                Invoice = invoice,
                DueDateUtc = ResolveDueDateUtc(invoice),
            })
            .OrderBy(item => item.DueDateUtc)
            .ThenByDescending(item => item.Invoice.InvoiceDateUtc)
            .ToArray();

        var aging = AgingBands
            .Select(band =>
            {
                var matches = overdueInvoices
                    .Where(item => IsInBand((now.Date - item.DueDateUtc.Date).Days, band.MinDays, band.MaxDays))
                    .ToArray();

                return new AccountsReceivableAgingBucketResponse(
                    band.Label,
                    matches.Length,
                    matches.Sum(item => item.Invoice.BalanceAmount),
                    band.Color);
            })
            .ToArray();

        var overdueQueue = overdueInvoices
            .Take(10)
            .Select(item => new AccountsReceivableInvoiceResponse(
                item.Invoice.InvoiceHeaderId.ToString(),
                item.Invoice.InvoiceNumber,
                item.Invoice.CustomerId.ToString(),
                item.Invoice.Customer?.CustomerName ?? string.Empty,
                item.DueDateUtc.ToString("O"),
                item.Invoice.BalanceAmount))
            .ToArray();

        var topOutstandingCustomers = overdueInvoices
            .GroupBy(item => new
            {
                item.Invoice.CustomerId,
                CustomerName = item.Invoice.Customer?.CustomerName ?? string.Empty,
            })
            .Select(group => new AccountsReceivableOutstandingCustomerResponse(
                group.Key.CustomerId.ToString(),
                group.Key.CustomerName,
                "individual",
                group.Sum(item => item.Invoice.BalanceAmount),
                group.Count()))
            .OrderByDescending(item => item.OutstandingAmount)
            .ThenByDescending(item => item.OverdueInvoices)
            .Take(5)
            .ToArray();

        return new AccountsReceivableDashboardResponse(
            aging,
            overdueQueue,
            topOutstandingCustomers,
            aging.Sum(item => item.Amount));
    }

    private static DateTime ResolveDueDateUtc(InvoiceHeader invoice)
    {
        return invoice.InvoiceDateUtc.AddDays(7);
    }

    private static bool IsInBand(int daysPastDue, int minDays, int? maxDays)
    {
        return daysPastDue >= minDays && (!maxDays.HasValue || daysPastDue <= maxDays.Value);
    }
}
