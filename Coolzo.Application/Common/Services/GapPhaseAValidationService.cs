using System.Security.Cryptography;
using System.Text;
using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;

namespace Coolzo.Application.Common.Services;

public sealed class GapPhaseAValidationService
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly IAmcRepository _amcRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentDateTime _currentDateTime;

    public GapPhaseAValidationService(
        IBookingRepository bookingRepository,
        IAmcRepository amcRepository,
        IAdminConfigurationRepository adminConfigurationRepository,
        ICurrentDateTime currentDateTime)
    {
        _bookingRepository = bookingRepository;
        _amcRepository = amcRepository;
        _adminConfigurationRepository = adminConfigurationRepository;
        _currentDateTime = currentDateTime;
    }

    public async Task ValidateBookingWindowAsync(SlotAvailability slotAvailability, string mobileNumber, long serviceId, CancellationToken cancellationToken)
    {
        if (await _bookingRepository.HasDuplicateBookingAsync(mobileNumber, slotAvailability.SlotAvailabilityId, serviceId, cancellationToken))
        {
            throw new AppException(ErrorCodes.BookingDuplicateDetected, "A duplicate booking already exists for this customer and slot.", 409);
        }

        if (slotAvailability.SlotConfiguration is not null &&
            await _bookingRepository.HasSlotConflictAsync(mobileNumber, slotAvailability.SlotDate, slotAvailability.SlotConfigurationId, cancellationToken))
        {
            throw new AppException(ErrorCodes.SlotConflictDetected, "The selected customer already has a booking in the same slot window.", 409);
        }

        if (await _adminConfigurationRepository.GetHolidayByDateAsync(slotAvailability.SlotDate, null, cancellationToken) is { IsActive: true })
        {
            throw new AppException(ErrorCodes.HolidayRestricted, "Bookings are blocked for the selected holiday.", 409);
        }

        var advanceLimitDays = await GetIntegerConfigurationAsync("Booking", "AdvanceLimitDays", 30, cancellationToken);
        if (slotAvailability.SlotDate > DateOnly.FromDateTime(_currentDateTime.UtcNow.AddDays(advanceLimitDays)))
        {
            throw new AppException(ErrorCodes.AdvanceBookingLimitExceeded, "The selected slot exceeds the allowed advance booking window.", 409);
        }

        if (slotAvailability.SlotConfiguration is not null)
        {
            var minimumLeadMinutes = await GetIntegerConfigurationAsync("Booking", "MinimumLeadMinutes", 120, cancellationToken);
            var slotDateTime = slotAvailability.SlotDate.ToDateTime(slotAvailability.SlotConfiguration.StartTime, DateTimeKind.Utc);

            if (slotDateTime < _currentDateTime.UtcNow.AddMinutes(minimumLeadMinutes))
            {
                throw new AppException(ErrorCodes.MinimumBookingTimeNotMet, "The selected slot does not satisfy the minimum booking lead time.", 409);
            }
        }
    }

    public async Task ValidateAmcDuplicateAsync(long customerId, long amcPlanId, CancellationToken cancellationToken)
    {
        if (await _amcRepository.HasActiveCustomerAmcAsync(customerId, amcPlanId, _currentDateTime.UtcNow, cancellationToken))
        {
            throw new AppException(ErrorCodes.CustomerAmcAlreadyExists, "An active AMC already exists for this customer and plan.", 409);
        }
    }

    public void ValidateInvoiceApproval(InvoiceHeader invoiceHeader)
    {
        if (invoiceHeader.QuotationHeader is null || invoiceHeader.QuotationHeader.CurrentStatus != Domain.Enums.QuotationStatus.Approved)
        {
            throw new AppException(ErrorCodes.QuotationApprovalRequired, "The invoice cannot proceed until its quotation is approved.", 409);
        }
    }

    public void ValidateRefundLimit(decimal requestedAmount, decimal maxAllowedAmount)
    {
        if (requestedAmount > maxAllowedAmount)
        {
            throw new AppException(ErrorCodes.RefundLimitExceeded, "The refund request exceeds the allowed refund limit.", 409);
        }
    }

    public void ValidatePaymentAmount(decimal paidAmount, decimal expectedAmount)
    {
        if (paidAmount != expectedAmount)
        {
            throw new AppException(ErrorCodes.PaymentAmountMismatch, "The payment amount does not match the expected verified amount.", 409);
        }
    }

    public void ValidateWebhookSignature(
        long invoiceId,
        decimal paidAmount,
        string referenceNumber,
        string? providedSignature,
        string secretKey)
    {
        if (string.IsNullOrWhiteSpace(providedSignature))
        {
            throw new AppException(ErrorCodes.InvalidWebhookSignature, "The webhook signature is missing.", 400);
        }

        var payload = $"{invoiceId}:{paidAmount:0.00}:{referenceNumber}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var hash = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));

        if (!hash.Equals(providedSignature, StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException(ErrorCodes.InvalidWebhookSignature, "The webhook signature is invalid.", 400);
        }
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
}
