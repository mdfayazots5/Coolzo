using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;

namespace Coolzo.Application.Common.Services;

public sealed class RefundApprovalRulesService
{
    private const decimal DefaultApprovalThreshold = 1500m;

    private readonly IAdminConfigurationRepository _adminConfigurationRepository;

    public RefundApprovalRulesService(IAdminConfigurationRepository adminConfigurationRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<RefundApprovalDecision> EvaluateAsync(
        InvoiceHeader invoiceHeader,
        decimal refundAmount,
        CancellationToken cancellationToken)
    {
        var threshold = await GetDecimalConfigurationAsync(
            "Refund",
            "ApprovalThreshold",
            DefaultApprovalThreshold,
            cancellationToken);
        var latestPaymentTransaction = invoiceHeader.PaymentTransactions
            .Where(transaction => !transaction.IsDeleted)
            .OrderByDescending(transaction => transaction.PaymentDateUtc)
            .FirstOrDefault();
        var effectivePaymentMethod = latestPaymentTransaction?.PaymentMethod ?? PaymentMethod.Cash;
        var approvalRequired = refundAmount >= threshold || effectivePaymentMethod == PaymentMethod.Cash;
        var financeApprovalRequired = effectivePaymentMethod == PaymentMethod.Cash;
        var defaultRefundMethod = effectivePaymentMethod == PaymentMethod.Cash
            ? RefundMethodType.CashReturn
            : RefundMethodType.OriginalPaymentMethod;

        return new RefundApprovalDecision(
            approvalRequired,
            financeApprovalRequired,
            threshold,
            latestPaymentTransaction?.PaymentTransactionId,
            effectivePaymentMethod,
            defaultRefundMethod);
    }

    private async Task<decimal> GetDecimalConfigurationAsync(
        string configurationGroup,
        string configurationKey,
        decimal defaultValue,
        CancellationToken cancellationToken)
    {
        var configuration = await _adminConfigurationRepository.GetSystemConfigurationByGroupAndKeyAsync(
            configurationGroup,
            configurationKey,
            null,
            cancellationToken);

        return configuration is not null && decimal.TryParse(configuration.ConfigurationValue, out var parsedValue)
            ? parsedValue
            : defaultValue;
    }
}

public sealed record RefundApprovalDecision(
    bool ApprovalRequired,
    bool FinanceApprovalRequired,
    decimal ApprovalThreshold,
    long? PaymentTransactionId,
    PaymentMethod PaymentMethod,
    RefundMethodType SuggestedRefundMethod);
