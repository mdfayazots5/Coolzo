using Coolzo.Contracts.Responses.GapPhaseD;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using DomainBooking = Coolzo.Domain.Entities.Booking;
using DomainServiceRequest = Coolzo.Domain.Entities.ServiceRequest;

namespace Coolzo.Application.Features.GapPhaseA.CancellationRefund;

internal static class PhaseDCancellationRefundSupport
{
    public static void ApplyBookingCancelled(DomainBooking booking, string remarks, DateTime now, string actor, string ipAddress)
    {
        if (booking.BookingStatus == BookingStatus.Cancelled)
        {
            return;
        }

        booking.BookingStatus = BookingStatus.Cancelled;
        booking.UpdatedBy = actor;
        booking.LastUpdated = now;
        booking.IPAddress = ipAddress;
        booking.BookingStatusHistories.Add(new BookingStatusHistory
        {
            BookingStatus = BookingStatus.Cancelled,
            Remarks = remarks,
            StatusDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = ipAddress
        });
    }

    public static void ApplyServiceRequestStatus(
        DomainServiceRequest serviceRequest,
        ServiceRequestStatus targetStatus,
        string remarks,
        DateTime now,
        string actor,
        string ipAddress)
    {
        serviceRequest.CurrentStatus = targetStatus;
        serviceRequest.UpdatedBy = actor;
        serviceRequest.LastUpdated = now;
        serviceRequest.IPAddress = ipAddress;
        serviceRequest.StatusHistories.Add(new ServiceRequestStatusHistory
        {
            Status = targetStatus,
            Remarks = remarks,
            StatusDateUtc = now,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = ipAddress
        });

        if (serviceRequest.JobCard is not null)
        {
            serviceRequest.JobCard.ExecutionTimelines.Add(new JobExecutionTimeline
            {
                Status = targetStatus,
                EventType = "StatusChanged",
                EventTitle = targetStatus.ToString(),
                Remarks = remarks,
                EventDateUtc = now,
                CreatedBy = actor,
                DateCreated = now,
                IPAddress = ipAddress
            });
        }
    }

    public static RefundMethodType ParseRefundMethod(string? refundMethod)
    {
        return Enum.TryParse<RefundMethodType>(refundMethod, true, out var parsedRefundMethod)
            ? parsedRefundMethod
            : RefundMethodType.OriginalPaymentMethod;
    }

    public static string BuildRefundRequestNumber(DateTime now)
    {
        return $"RFD-{now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";
    }

    public static RefundStatusHistory BuildRefundStatusHistory(
        RefundRequest refundRequest,
        string fromStatus,
        string toStatus,
        long changedByUserId,
        string remarks,
        DateTime now,
        string actor,
        string ipAddress)
    {
        return new RefundStatusHistory
        {
            RefundRequestId = refundRequest.RefundRequestId,
            RefundRequest = refundRequest,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedByUserId = changedByUserId,
            ChangedOn = now,
            Remarks = remarks,
            CreatedBy = actor,
            DateCreated = now,
            IPAddress = ipAddress
        };
    }

    public static CancellationListItemResponse MapCancellationListItem(CancellationRecord cancellationRecord, RefundRequest? refundRequest)
    {
        return new CancellationListItemResponse(
            cancellationRecord.CancellationRecordId,
            cancellationRecord.BookingId,
            cancellationRecord.ServiceRequestId,
            ResolveReferenceNumber(cancellationRecord),
            NormalizeCancellationStatus(cancellationRecord.CancellationStatus),
            cancellationRecord.CancellationSource,
            ResolveCancellationReasonCode(cancellationRecord),
            cancellationRecord.CancellationFeeAmount,
            cancellationRecord.RefundEligibleAmount,
            cancellationRecord.CancelledByRole,
            cancellationRecord.DateCreated,
            refundRequest?.RefundRequestId,
            refundRequest?.RefundStatus.ToString());
    }

    public static CancellationDetailResponse MapCancellationDetail(CancellationRecord cancellationRecord, RefundRequest? refundRequest)
    {
        return new CancellationDetailResponse(
            cancellationRecord.CancellationRecordId,
            cancellationRecord.BookingId,
            cancellationRecord.ServiceRequestId,
            cancellationRecord.CancelledByUserId,
            cancellationRecord.CancelledByRole,
            cancellationRecord.CancellationSource,
            ResolveCancellationReasonCode(cancellationRecord),
            ResolveCancellationReasonText(cancellationRecord),
            cancellationRecord.TimeToSlotMinutes,
            cancellationRecord.CancellationFeeAmount,
            cancellationRecord.RefundEligibleAmount,
            NormalizeCancellationStatus(cancellationRecord.CancellationStatus),
            cancellationRecord.PolicyCode,
            cancellationRecord.Comments ?? string.Empty,
            cancellationRecord.RequiresApproval,
            cancellationRecord.DateCreated,
            refundRequest?.RefundRequestId,
            refundRequest?.RefundStatus.ToString());
    }

    public static RefundListItemResponse MapRefundListItem(RefundRequest refundRequest)
    {
        return new RefundListItemResponse(
            refundRequest.RefundRequestId,
            refundRequest.RefundRequestNo,
            refundRequest.CancellationRecordId,
            refundRequest.InvoiceHeaderId,
            refundRequest.PaymentTransactionId,
            refundRequest.RefundAmount,
            refundRequest.RefundMethod.ToString(),
            refundRequest.RefundStatus.ToString(),
            refundRequest.ApprovalRequiredFlag,
            refundRequest.DateCreated,
            refundRequest.ProcessedOn);
    }

    public static RefundDetailResponse MapRefundDetail(RefundRequest refundRequest)
    {
        return new RefundDetailResponse(
            refundRequest.RefundRequestId,
            refundRequest.RefundRequestNo,
            refundRequest.CancellationRecordId,
            refundRequest.InvoiceHeaderId,
            refundRequest.PaymentTransactionId,
            refundRequest.RefundAmount,
            refundRequest.RequestedAmount,
            refundRequest.ApprovedAmount,
            refundRequest.MaxAllowedAmount,
            refundRequest.RefundMethod.ToString(),
            refundRequest.RefundReason,
            refundRequest.RefundStatus.ToString(),
            refundRequest.ApprovalRequiredFlag,
            refundRequest.ApprovedByUserId,
            refundRequest.ApprovedDateUtc,
            refundRequest.ProcessedOn,
            refundRequest.Approvals
                .OrderBy(item => item.ApprovalLevel)
                .Select(item => new RefundApprovalHistoryResponse(
                    item.RefundApprovalId,
                    item.ApprovalLevel,
                    item.ApproverUserId,
                    item.ApprovalStatus,
                    item.ApprovalRemarks,
                    item.ApprovedOn))
                .ToArray(),
            refundRequest.StatusHistory
                .OrderBy(item => item.ChangedOn)
                .Select(item => new RefundStatusHistoryResponse(
                    item.RefundStatusHistoryId,
                    item.FromStatus,
                    item.ToStatus,
                    item.ChangedByUserId,
                    item.ChangedOn,
                    item.Remarks))
                .ToArray());
    }

    public static CustomerRefundStatusResponse MapCustomerRefundStatus(RefundRequest refundRequest)
    {
        return new CustomerRefundStatusResponse(
            refundRequest.RefundRequestId,
            refundRequest.RefundRequestNo,
            refundRequest.RefundAmount,
            refundRequest.RefundMethod.ToString(),
            refundRequest.RefundStatus.ToString(),
            refundRequest.DateCreated,
            refundRequest.ProcessedOn);
    }

    public static CustomerAbsentDetailResponse MapCustomerAbsentDetail(CustomerAbsentRecord customerAbsentRecord)
    {
        return new CustomerAbsentDetailResponse(
            customerAbsentRecord.CustomerAbsentRecordId,
            customerAbsentRecord.ServiceRequestId,
            customerAbsentRecord.ServiceRequest?.ServiceRequestNumber ?? string.Empty,
            customerAbsentRecord.ServiceRequest?.BookingId ?? 0,
            customerAbsentRecord.ServiceRequest?.Booking?.BookingReference ?? string.Empty,
            customerAbsentRecord.TechnicianId,
            customerAbsentRecord.Technician?.TechnicianName ?? string.Empty,
            customerAbsentRecord.MarkedOn,
            customerAbsentRecord.AttemptCount,
            customerAbsentRecord.ContactAttemptLog,
            customerAbsentRecord.AbsentReasonCode,
            customerAbsentRecord.AbsentReasonText,
            customerAbsentRecord.CustomerAbsentStatus.ToString(),
            customerAbsentRecord.ServiceRequest?.CurrentStatus.ToString() ?? string.Empty,
            customerAbsentRecord.ResolutionRemarks,
            customerAbsentRecord.ResolvedOn);
    }

    public static string NormalizeCancellationStatus(CancellationStatus cancellationStatus)
    {
        return cancellationStatus switch
        {
            CancellationStatus.Approved => CancellationStatus.Cancelled.ToString(),
            CancellationStatus.Completed => CancellationStatus.Closed.ToString(),
            _ => cancellationStatus.ToString()
        };
    }

    private static string ResolveReferenceNumber(CancellationRecord cancellationRecord)
    {
        if (cancellationRecord.ServiceRequest is not null)
        {
            return cancellationRecord.ServiceRequest.ServiceRequestNumber;
        }

        return cancellationRecord.Booking?.BookingReference ?? cancellationRecord.CancellationRecordId.ToString();
    }

    private static string ResolveCancellationReasonCode(CancellationRecord cancellationRecord)
    {
        return string.IsNullOrWhiteSpace(cancellationRecord.CancellationReasonCode)
            ? cancellationRecord.ReasonCode
            : cancellationRecord.CancellationReasonCode;
    }

    private static string ResolveCancellationReasonText(CancellationRecord cancellationRecord)
    {
        return string.IsNullOrWhiteSpace(cancellationRecord.CancellationReasonText)
            ? cancellationRecord.ReasonDescription
            : cancellationRecord.CancellationReasonText;
    }
}
