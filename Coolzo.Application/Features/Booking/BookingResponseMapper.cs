using Coolzo.Application.Features.TechnicianJob;
using Coolzo.Contracts.Responses.Booking;
using Coolzo.Contracts.Responses.FieldExecution;
using Coolzo.Domain.Entities;
using DomainBooking = Coolzo.Domain.Entities.Booking;

namespace Coolzo.Application.Features.Booking;

internal static class BookingResponseMapper
{
    public static BookingSummaryResponse ToSummary(DomainBooking booking)
    {
        var slotDate = booking.SlotAvailability?.SlotDate ?? DateOnly.FromDateTime(booking.BookingDateUtc);
        var slotLabel = booking.SlotAvailability?.SlotConfiguration?.SlotLabel ?? "Preferred Slot";

        return new BookingSummaryResponse(
            booking.BookingId,
            booking.BookingReference,
            booking.BookingStatus.ToString(),
            booking.ServiceNameSnapshot,
            booking.CustomerNameSnapshot,
            booking.MobileNumberSnapshot,
            slotDate,
            slotLabel,
            BuildAddressSummary(booking),
            booking.EstimatedPrice);
    }

    public static BookingDetailResponse ToDetail(
        DomainBooking booking,
        IReadOnlyCollection<ServiceChecklistMaster> checklistMasters)
    {
        var slotDate = booking.SlotAvailability?.SlotDate ?? DateOnly.FromDateTime(booking.BookingDateUtc);
        var slotLabel = booking.SlotAvailability?.SlotConfiguration?.SlotLabel ?? "Preferred Slot";
        var quotation = booking.ServiceRequest?.JobCard?.Quotations
            .Where(item => !item.IsDeleted)
            .OrderByDescending(item => item.QuotationDateUtc)
            .FirstOrDefault();
        var invoice = quotation?.InvoiceHeader is { IsDeleted: false } ? quotation.InvoiceHeader : null;

        return new BookingDetailResponse(
            booking.BookingId,
            booking.BookingReference,
            booking.BookingStatus.ToString(),
            booking.SourceChannel.ToString(),
            booking.IsGuestBooking,
            booking.BookingDateUtc,
            booking.ServiceNameSnapshot,
            booking.CustomerNameSnapshot,
            booking.MobileNumberSnapshot,
            booking.EmailAddressSnapshot,
            BuildAddressSummary(booking),
            booking.ZoneNameSnapshot,
            slotDate,
            slotLabel,
            booking.EstimatedPrice,
            booking.ServiceRequest?.ServiceRequestId,
            booking.ServiceRequest?.ServiceRequestNumber,
            booking.ServiceRequest?.CurrentStatus.ToString(),
            GetActiveAssignment(booking)?.TechnicianId,
            GetActiveAssignment(booking)?.Technician?.TechnicianName,
            booking.ServiceRequest?.JobCard?.JobCardId,
            booking.ServiceRequest?.JobCard?.JobCardNumber,
            quotation?.QuotationHeaderId,
            quotation?.QuotationNumber,
            quotation?.CurrentStatus.ToString(),
            invoice?.InvoiceHeaderId,
            invoice?.InvoiceNumber,
            invoice?.CurrentStatus.ToString(),
            invoice?.GrandTotalAmount,
            invoice?.BalanceAmount,
            string.IsNullOrWhiteSpace(booking.ServiceRequest?.JobCard?.CompletionSummary) ? null : booking.ServiceRequest.JobCard.CompletionSummary,
            booking.ServiceRequest is null
                ? Array.Empty<JobExecutionTimelineItemResponse>()
                : TechnicianJobResponseMapper.ToExecutionTimeline(booking.ServiceRequest),
            booking.ServiceRequest is null
                ? Array.Empty<JobExecutionNoteResponse>()
                : TechnicianJobResponseMapper.ToExecutionNotes(booking.ServiceRequest.JobCard, true),
            booking.BookingLines
                .Select(line => new BookingLineResponse(
                    line.Service?.ServiceName ?? booking.ServiceNameSnapshot,
                    line.AcType?.AcTypeName ?? string.Empty,
                    line.Tonnage?.TonnageName ?? string.Empty,
                    line.Brand?.BrandName ?? string.Empty,
                    line.ModelName,
                    line.IssueNotes,
                    line.Quantity,
                    line.UnitPrice,
                    line.LineTotal))
                .ToArray(),
            booking.BookingStatusHistories
                .OrderBy(history => history.StatusDateUtc)
                .Select(history => new BookingStatusHistoryResponse(
                    history.BookingStatus.ToString(),
                    history.Remarks,
                    history.StatusDateUtc))
                .ToArray());
    }

    public static BookingListItemResponse ToListItem(DomainBooking booking)
    {
        var slotDate = booking.SlotAvailability?.SlotDate ?? DateOnly.FromDateTime(booking.BookingDateUtc);
        var slotLabel = booking.SlotAvailability?.SlotConfiguration?.SlotLabel ?? "Preferred Slot";

        return new BookingListItemResponse(
            booking.BookingId,
            booking.BookingReference,
            booking.BookingStatus.ToString(),
            booking.ServiceNameSnapshot,
            booking.CustomerNameSnapshot,
            booking.MobileNumberSnapshot,
            slotDate,
            slotLabel,
            booking.SourceChannel.ToString(),
            booking.BookingDateUtc,
            booking.ServiceRequest?.CurrentStatus.ToString(),
            GetActiveAssignment(booking)?.Technician?.TechnicianName);
    }

    private static string BuildAddressSummary(DomainBooking booking)
    {
        return TechnicianJobResponseMapper.BuildAddressSummary(booking);
    }

    private static ServiceRequestAssignment? GetActiveAssignment(DomainBooking booking)
    {
        return booking.ServiceRequest?.Assignments
            .Where(assignment => assignment.IsActiveAssignment && !assignment.IsDeleted)
            .OrderByDescending(assignment => assignment.AssignedDateUtc)
            .FirstOrDefault();
    }
}
