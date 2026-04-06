using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Models;

namespace Coolzo.Application.Common.Services;

public sealed class CancellationPolicyEvaluationService
{
    private const int DefaultFreeCancellationMinutes = 24 * 60;
    private const int DefaultHalfFeeMinutes = 2 * 60;
    private const int DefaultCustomerCutoffMinutes = 2 * 60;

    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IGapPhaseARepository _gapPhaseARepository;

    public CancellationPolicyEvaluationService(
        IGapPhaseARepository gapPhaseARepository,
        IAdminConfigurationRepository adminConfigurationRepository,
        ICurrentDateTime currentDateTime)
    {
        _gapPhaseARepository = gapPhaseARepository;
        _adminConfigurationRepository = adminConfigurationRepository;
        _currentDateTime = currentDateTime;
    }

    public async Task<CancellationPolicyEvaluationResult> EvaluateAsync(
        Booking booking,
        ServiceRequest? serviceRequest,
        CancellationToken cancellationToken)
    {
        var scheduledStartUtc = ResolveScheduledStartUtc(booking);
        var now = _currentDateTime.UtcNow;
        var timeToSlotMinutes = (int)Math.Floor((scheduledStartUtc - now).TotalMinutes);
        var normalizedTimeToSlotMinutes = Math.Max(timeToSlotMinutes, 0);
        var paidAmount = ResolvePaidAmount(booking);
        var feeBaseAmount = paidAmount > 0 ? paidAmount : booking.EstimatedPrice;
        var isTechnicianDispatched = serviceRequest?.CurrentStatus is
            ServiceRequestStatus.Assigned or
            ServiceRequestStatus.EnRoute or
            ServiceRequestStatus.Reached or
            ServiceRequestStatus.WorkStarted or
            ServiceRequestStatus.WorkInProgress or
            ServiceRequestStatus.WorkCompletedPendingSubmission or
            ServiceRequestStatus.SubmittedForClosure or
            ServiceRequestStatus.CustomerAbsent;
        var policies = await _gapPhaseARepository.GetCancellationPoliciesAsync(
            booking.BranchId,
            booking.CompanyId,
            booking.SiteId,
            string.Empty,
            cancellationToken);

        var applicablePolicy = SelectPolicy(policies, normalizedTimeToSlotMinutes, isTechnicianDispatched)
            ?? BuildDefaultPolicy(normalizedTimeToSlotMinutes, isTechnicianDispatched);
        var cancellationFee = Math.Round(feeBaseAmount * (applicablePolicy.FeePercent / 100m), 2, MidpointRounding.AwayFromZero);
        var refundEligibleAmount = Math.Max(paidAmount - cancellationFee, 0m);
        var customerCutoffMinutes = await GetIntegerConfigurationAsync(
            "CancellationPolicy",
            "CustomerCutoffMinutes",
            DefaultCustomerCutoffMinutes,
            cancellationToken);
        var canCustomerCancel = normalizedTimeToSlotMinutes >= customerCutoffMinutes && !isTechnicianDispatched;
        var customerDenialReason = canCustomerCancel
            ? string.Empty
            : "Customer self-cancellation is blocked after the configured cutoff or once a technician is dispatched.";

        return new CancellationPolicyEvaluationResult(
            applicablePolicy.PolicyCode,
            applicablePolicy.PolicyName,
            applicablePolicy.Description,
            normalizedTimeToSlotMinutes,
            paidAmount,
            cancellationFee,
            refundEligibleAmount,
            applicablePolicy.RequiresManagerApproval || isTechnicianDispatched,
            canCustomerCancel,
            customerDenialReason,
            scheduledStartUtc,
            applicablePolicy.FeePercent,
            isTechnicianDispatched);
    }

    private async Task<int> GetIntegerConfigurationAsync(
        string configurationGroup,
        string configurationKey,
        int defaultValue,
        CancellationToken cancellationToken)
    {
        var configuration = await _adminConfigurationRepository.GetSystemConfigurationByGroupAndKeyAsync(
            configurationGroup,
            configurationKey,
            null,
            cancellationToken);

        return configuration is not null && int.TryParse(configuration.ConfigurationValue, out var parsedValue)
            ? parsedValue
            : defaultValue;
    }

    private static DateTime ResolveScheduledStartUtc(Booking booking)
    {
        if (booking.SlotAvailability?.SlotConfiguration is not null)
        {
            return booking.SlotAvailability.SlotDate.ToDateTime(booking.SlotAvailability.SlotConfiguration.StartTime, DateTimeKind.Utc);
        }

        return booking.BookingDateUtc;
    }

    private static decimal ResolvePaidAmount(Booking booking)
    {
        var latestInvoice = booking.ServiceRequest?.JobCard?.Quotations
            .Where(item => !item.IsDeleted && item.InvoiceHeader is { IsDeleted: false })
            .OrderByDescending(item => item.QuotationDateUtc)
            .Select(item => item.InvoiceHeader!)
            .FirstOrDefault();

        if (latestInvoice is null)
        {
            return 0m;
        }

        var transactionPaidAmount = latestInvoice.PaymentTransactions
            .Where(transaction => !transaction.IsDeleted)
            .Sum(transaction => transaction.PaidAmount);

        return transactionPaidAmount > 0 ? transactionPaidAmount : latestInvoice.PaidAmount;
    }

    private static CancellationPolicy? SelectPolicy(
        IReadOnlyCollection<CancellationPolicy> policies,
        int timeToSlotMinutes,
        bool isTechnicianDispatched)
    {
        return policies
            .Where(policy =>
                policy.AppliesWhenTechnicianDispatched == isTechnicianDispatched &&
                (!policy.MinTimeToSlotMinutes.HasValue || timeToSlotMinutes >= policy.MinTimeToSlotMinutes.Value) &&
                (!policy.MaxTimeToSlotMinutes.HasValue || timeToSlotMinutes < policy.MaxTimeToSlotMinutes.Value))
            .OrderByDescending(policy => policy.BranchId)
            .ThenBy(policy => policy.MinTimeToSlotMinutes ?? int.MinValue)
            .FirstOrDefault();
    }

    private static CancellationPolicy BuildDefaultPolicy(int timeToSlotMinutes, bool isTechnicianDispatched)
    {
        if (isTechnicianDispatched || timeToSlotMinutes < DefaultHalfFeeMinutes)
        {
            return new CancellationPolicy
            {
                PolicyCode = "FULL_FEE",
                PolicyName = "Late cancellation or dispatched visit",
                Description = "100% cancellation fee applies once the visit is inside two hours or a technician is dispatched.",
                FeePercent = 100m,
                RequiresManagerApproval = isTechnicianDispatched,
                AppliesWhenTechnicianDispatched = isTechnicianDispatched
            };
        }

        if (timeToSlotMinutes < DefaultFreeCancellationMinutes)
        {
            return new CancellationPolicy
            {
                PolicyCode = "HALF_FEE",
                PolicyName = "Short notice cancellation",
                Description = "50% cancellation fee applies between two and twenty-four hours before the appointment.",
                FeePercent = 50m
            };
        }

        return new CancellationPolicy
        {
            PolicyCode = "FREE_CANCELLATION",
            PolicyName = "Free cancellation",
            Description = "No cancellation fee applies when the appointment is more than twenty-four hours away.",
            FeePercent = 0m
        };
    }
}

public sealed record CancellationPolicyEvaluationResult(
    string PolicyCode,
    string PolicyName,
    string PolicyDescription,
    int TimeToSlotMinutes,
    decimal PaidAmount,
    decimal CancellationFee,
    decimal RefundEligibleAmount,
    bool ApprovalRequired,
    bool CanCustomerCancel,
    string CustomerDenialReason,
    DateTime ScheduledStartUtc,
    decimal FeePercent,
    bool IsTechnicianDispatched);
