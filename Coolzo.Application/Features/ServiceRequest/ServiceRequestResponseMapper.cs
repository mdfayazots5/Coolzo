using Coolzo.Application.Features.TechnicianJob;
using Coolzo.Contracts.Responses.Operations;
using Coolzo.Domain.Entities;
using DomainBooking = Coolzo.Domain.Entities.Booking;
using DomainServiceRequest = Coolzo.Domain.Entities.ServiceRequest;

namespace Coolzo.Application.Features.ServiceRequest;

internal static class ServiceRequestResponseMapper
{
    public static ServiceRequestDetailResponse ToDetail(
        DomainServiceRequest serviceRequest,
        IReadOnlyCollection<ServiceChecklistMaster> checklistMasters)
    {
        var booking = serviceRequest.Booking;
        var primaryLine = booking?.BookingLines.OrderBy(line => line.BookingLineId).FirstOrDefault();
        var activeAssignment = TechnicianJobResponseMapper.GetActiveAssignment(serviceRequest);
        var slotDate = booking?.SlotAvailability?.SlotDate ?? DateOnly.FromDateTime(serviceRequest.ServiceRequestDateUtc);
        var slotLabel = booking?.SlotAvailability?.SlotConfiguration?.SlotLabel ?? "Preferred Slot";
        var quotation = serviceRequest.JobCard?.Quotations
            .Where(item => !item.IsDeleted)
            .OrderByDescending(item => item.QuotationDateUtc)
            .FirstOrDefault();
        var invoice = quotation?.InvoiceHeader is { IsDeleted: false } ? quotation.InvoiceHeader : null;

        return new ServiceRequestDetailResponse(
            serviceRequest.ServiceRequestId,
            serviceRequest.ServiceRequestNumber,
            serviceRequest.BookingId,
            booking?.BookingReference ?? string.Empty,
            booking?.BookingStatus.ToString() ?? string.Empty,
            serviceRequest.CurrentStatus.ToString(),
            serviceRequest.ServiceRequestDateUtc,
            booking?.SourceChannel.ToString() ?? string.Empty,
            booking?.CustomerNameSnapshot ?? string.Empty,
            booking?.MobileNumberSnapshot ?? string.Empty,
            booking?.EmailAddressSnapshot ?? string.Empty,
            BuildAddressSummary(booking),
            booking?.ZoneNameSnapshot ?? string.Empty,
            slotDate,
            slotLabel,
            primaryLine?.Service?.ServiceName ?? booking?.ServiceNameSnapshot ?? string.Empty,
            primaryLine?.AcType?.AcTypeName ?? string.Empty,
            primaryLine?.Tonnage?.TonnageName ?? string.Empty,
            primaryLine?.Brand?.BrandName ?? string.Empty,
            primaryLine?.ModelName ?? string.Empty,
            primaryLine?.IssueNotes ?? string.Empty,
            booking?.EstimatedPrice ?? 0.00m,
            activeAssignment?.TechnicianId,
            activeAssignment?.Technician?.TechnicianCode,
            activeAssignment?.Technician?.TechnicianName,
            activeAssignment?.Technician?.MobileNumber,
            activeAssignment?.AssignmentRemarks,
            TechnicianJobResponseMapper.ToJobCardSummary(serviceRequest.JobCard),
            quotation?.QuotationHeaderId,
            quotation?.QuotationNumber,
            quotation?.CurrentStatus.ToString(),
            invoice?.InvoiceHeaderId,
            invoice?.InvoiceNumber,
            invoice?.CurrentStatus.ToString(),
            invoice?.GrandTotalAmount,
            invoice?.BalanceAmount,
            TechnicianJobResponseMapper.ToDiagnosisSummary(serviceRequest.JobCard),
            TechnicianJobResponseMapper.ToChecklistSummary(serviceRequest, checklistMasters),
            TechnicianJobResponseMapper.ToExecutionNotes(serviceRequest.JobCard, false),
            TechnicianJobResponseMapper.ToAttachments(serviceRequest.JobCard),
            TechnicianJobResponseMapper.ToExecutionTimeline(serviceRequest),
            serviceRequest.StatusHistories
                .Where(history => !history.IsDeleted)
                .OrderBy(history => history.StatusDateUtc)
                .Select(history => new ServiceRequestStatusHistoryResponse(
                    history.Status.ToString(),
                    history.Remarks,
                    history.StatusDateUtc))
                .ToArray(),
            ToAssignmentHistory(serviceRequest));
    }

    public static ServiceRequestListItemResponse ToListItem(DomainServiceRequest serviceRequest)
    {
        var booking = serviceRequest.Booking;
        var activeAssignment = TechnicianJobResponseMapper.GetActiveAssignment(serviceRequest);
        var slotDate = booking?.SlotAvailability?.SlotDate ?? DateOnly.FromDateTime(serviceRequest.ServiceRequestDateUtc);
        var slotLabel = booking?.SlotAvailability?.SlotConfiguration?.SlotLabel ?? "Preferred Slot";

        return new ServiceRequestListItemResponse(
            serviceRequest.ServiceRequestId,
            serviceRequest.ServiceRequestNumber,
            serviceRequest.BookingId,
            booking?.BookingReference ?? string.Empty,
            booking?.CustomerNameSnapshot ?? string.Empty,
            booking?.ServiceNameSnapshot ?? string.Empty,
            serviceRequest.CurrentStatus.ToString(),
            activeAssignment?.Technician?.TechnicianName,
            slotDate,
            slotLabel,
            serviceRequest.ServiceRequestDateUtc);
    }

    public static IReadOnlyCollection<AssignmentHistoryItemResponse> ToAssignmentHistory(DomainServiceRequest serviceRequest)
    {
        return serviceRequest.AssignmentLogs
            .Where(log => !log.IsDeleted)
            .OrderByDescending(log => log.ActionDateUtc)
            .Select(log => new AssignmentHistoryItemResponse(
                log.ActionName,
                log.PreviousTechnician?.TechnicianName,
                log.CurrentTechnician?.TechnicianName ?? string.Empty,
                log.Remarks,
                log.ActionDateUtc))
            .ToArray();
    }

    private static string BuildAddressSummary(DomainBooking? booking)
    {
        return TechnicianJobResponseMapper.BuildAddressSummary(booking);
    }
}
