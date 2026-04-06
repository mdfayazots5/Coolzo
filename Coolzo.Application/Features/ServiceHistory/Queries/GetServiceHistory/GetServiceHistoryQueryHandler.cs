using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Features.Amc;
using Coolzo.Contracts.Responses.ServiceHistory;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;
using DomainBooking = Coolzo.Domain.Entities.Booking;

namespace Coolzo.Application.Features.ServiceHistory.Queries.GetServiceHistory;

public sealed class GetServiceHistoryQueryHandler : IRequestHandler<GetServiceHistoryQuery, IReadOnlyCollection<ServiceHistoryItemResponse>>
{
    private readonly IAmcRepository _amcRepository;
    private readonly ServiceLifecycleAccessService _serviceLifecycleAccessService;

    public GetServiceHistoryQueryHandler(
        IAmcRepository amcRepository,
        ServiceLifecycleAccessService serviceLifecycleAccessService)
    {
        _amcRepository = amcRepository;
        _serviceLifecycleAccessService = serviceLifecycleAccessService;
    }

    public async Task<IReadOnlyCollection<ServiceHistoryItemResponse>> Handle(GetServiceHistoryQuery request, CancellationToken cancellationToken)
    {
        await _serviceLifecycleAccessService.EnsureCustomerReadAccessAsync(request.CustomerId, cancellationToken);

        var bookings = await _amcRepository.GetBookingsByCustomerIdAsync(request.CustomerId, cancellationToken);
        var customerAmcs = await _amcRepository.GetCustomerAmcByCustomerIdAsync(request.CustomerId, cancellationToken);
        var warrantyClaims = await _amcRepository.GetWarrantyClaimsByCustomerIdAsync(request.CustomerId, cancellationToken);
        var revisitRequests = await _amcRepository.GetRevisitRequestsByCustomerIdAsync(request.CustomerId, cancellationToken);

        var historyItems = new List<ServiceHistoryItemResponse>();

        foreach (var booking in bookings)
        {
            historyItems.Add(new ServiceHistoryItemResponse(
                "Booking",
                booking.BookingReference,
                ResolveServiceName(booking),
                booking.BookingStatus.ToString(),
                booking.BookingDateUtc,
                BuildAddressSummary(booking),
                booking.EstimatedPrice,
                booking.BookingId,
                null,
                null,
                null,
                null,
                null));

            if (booking.ServiceRequest is not null && !booking.ServiceRequest.IsDeleted)
            {
                historyItems.Add(new ServiceHistoryItemResponse(
                    "ServiceRequest",
                    booking.ServiceRequest.ServiceRequestNumber,
                    ResolveServiceName(booking),
                    booking.ServiceRequest.CurrentStatus.ToString(),
                    booking.ServiceRequest.ServiceRequestDateUtc,
                    $"Service request raised for booking {booking.BookingReference}.",
                    null,
                    booking.BookingId,
                    booking.ServiceRequest.ServiceRequestId,
                    null,
                    null,
                    null,
                    null));
            }

            var jobCard = booking.ServiceRequest?.JobCard;

            if (jobCard is not null && !jobCard.IsDeleted)
            {
                historyItems.Add(new ServiceHistoryItemResponse(
                    "JobCard",
                    jobCard.JobCardNumber,
                    ResolveServiceName(booking),
                    booking.ServiceRequest?.CurrentStatus.ToString() ?? "Assigned",
                    jobCard.WorkCompletedDateUtc ??
                        jobCard.SubmittedForClosureDateUtc ??
                        jobCard.WorkStartedDateUtc ??
                        booking.ServiceRequest?.ServiceRequestDateUtc ??
                        booking.BookingDateUtc,
                    string.IsNullOrWhiteSpace(jobCard.CompletionSummary)
                        ? "Job execution completed for the service request."
                        : jobCard.CompletionSummary,
                    null,
                    booking.BookingId,
                    booking.ServiceRequest?.ServiceRequestId,
                    jobCard.JobCardId,
                    null,
                    null,
                    null));

                foreach (var quotation in jobCard.Quotations.Where(item => !item.IsDeleted))
                {
                    if (quotation.InvoiceHeader is null || quotation.InvoiceHeader.IsDeleted)
                    {
                        continue;
                    }

                    historyItems.Add(new ServiceHistoryItemResponse(
                        "Invoice",
                        quotation.InvoiceHeader.InvoiceNumber,
                        ResolveServiceName(booking),
                        quotation.InvoiceHeader.CurrentStatus.ToString(),
                        quotation.InvoiceHeader.InvoiceDateUtc,
                        $"Invoice generated from quotation {quotation.QuotationNumber}.",
                        quotation.InvoiceHeader.GrandTotalAmount,
                        booking.BookingId,
                        booking.ServiceRequest?.ServiceRequestId,
                        jobCard.JobCardId,
                        quotation.InvoiceHeader.InvoiceHeaderId,
                        null,
                        null));
                }
            }
        }

        foreach (var customerAmc in customerAmcs.Where(item => !item.IsDeleted))
        {
            historyItems.Add(new ServiceHistoryItemResponse(
                "AMC",
                customerAmc.InvoiceHeader?.InvoiceNumber ?? customerAmc.CustomerAmcId.ToString(),
                customerAmc.AmcPlan?.PlanName ?? "AMC Plan",
                customerAmc.CurrentStatus.ToString(),
                customerAmc.StartDateUtc,
                $"{customerAmc.TotalVisitCount} visits scheduled until {customerAmc.EndDateUtc:d}.",
                customerAmc.PriceAmount,
                null,
                null,
                customerAmc.JobCardId,
                customerAmc.InvoiceHeaderId,
                customerAmc.CustomerAmcId,
                null));
        }

        foreach (var warrantyClaim in warrantyClaims.Where(item => !item.IsDeleted))
        {
            historyItems.Add(new ServiceHistoryItemResponse(
                "WarrantyClaim",
                warrantyClaim.InvoiceHeader?.InvoiceNumber ?? warrantyClaim.WarrantyClaimId.ToString(),
                warrantyClaim.WarrantyRule?.RuleName ?? "Warranty Claim",
                warrantyClaim.CurrentStatus.ToString(),
                warrantyClaim.ClaimDateUtc,
                warrantyClaim.ClaimRemarks,
                null,
                warrantyClaim.JobCard?.ServiceRequest?.BookingId,
                warrantyClaim.JobCard?.ServiceRequestId,
                warrantyClaim.JobCardId,
                warrantyClaim.InvoiceHeaderId,
                null,
                warrantyClaim.RevisitRequest?.RevisitRequestId));
        }

        foreach (var revisitRequest in revisitRequests.Where(item => !item.IsDeleted))
        {
            historyItems.Add(new ServiceHistoryItemResponse(
                "Revisit",
                revisitRequest.Booking?.BookingReference ?? revisitRequest.RevisitRequestId.ToString(),
                revisitRequest.RevisitType.ToString(),
                revisitRequest.CurrentStatus.ToString(),
                revisitRequest.RequestedDateUtc,
                revisitRequest.IssueSummary,
                revisitRequest.ChargeAmount,
                revisitRequest.BookingId,
                revisitRequest.ServiceRequestId,
                revisitRequest.OriginalJobCardId,
                revisitRequest.WarrantyClaim?.InvoiceHeaderId,
                revisitRequest.CustomerAmcId,
                revisitRequest.RevisitRequestId));
        }

        return historyItems
            .OrderByDescending(item => item.EventDateUtc)
            .ThenByDescending(item => item.ReferenceNumber)
            .ToArray();
    }

    private static string ResolveServiceName(DomainBooking booking)
    {
        return booking.BookingLines
            .Where(line => !line.IsDeleted)
            .OrderBy(line => line.BookingLineId)
            .Select(line => line.Service?.ServiceName ?? booking.ServiceNameSnapshot)
            .FirstOrDefault() ?? booking.ServiceNameSnapshot;
    }

    private static string BuildAddressSummary(DomainBooking booking)
    {
        var parts = new[]
        {
            booking.AddressLine1Snapshot,
            booking.AddressLine2Snapshot,
            booking.LandmarkSnapshot,
            booking.CityNameSnapshot,
            booking.PincodeSnapshot
        }
        .Where(value => !string.IsNullOrWhiteSpace(value));

        return string.Join(", ", parts);
    }
}
