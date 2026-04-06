using Coolzo.Contracts.Responses.FieldExecution;
using Coolzo.Contracts.Responses.TechnicianJobs;
using Coolzo.Contracts.Responses.Support;
using Coolzo.Application.Features.Support;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using DomainBooking = Coolzo.Domain.Entities.Booking;
using DomainServiceRequest = Coolzo.Domain.Entities.ServiceRequest;

namespace Coolzo.Application.Features.TechnicianJob;

internal static class TechnicianJobResponseMapper
{
    public static TechnicianJobListItemResponse ToListItem(
        DomainServiceRequest serviceRequest,
        string lifecycleType,
        string lifecycleLabel)
    {
        var booking = serviceRequest.Booking;
        var slotDate = booking?.SlotAvailability?.SlotDate ?? DateOnly.FromDateTime(serviceRequest.ServiceRequestDateUtc);
        var slotLabel = booking?.SlotAvailability?.SlotConfiguration?.SlotLabel ?? "Preferred Slot";

        return new TechnicianJobListItemResponse(
            serviceRequest.ServiceRequestId,
            serviceRequest.JobCard?.JobCardId,
            serviceRequest.ServiceRequestNumber,
            serviceRequest.JobCard?.JobCardNumber,
            lifecycleType,
            lifecycleLabel,
            booking?.BookingReference ?? string.Empty,
            booking?.CustomerNameSnapshot ?? string.Empty,
            booking?.MobileNumberSnapshot ?? string.Empty,
            BuildAddressSummary(booking),
            booking?.ServiceNameSnapshot ?? string.Empty,
            serviceRequest.CurrentStatus.ToString(),
            slotDate,
            slotLabel);
    }

    public static TechnicianJobDetailResponse ToDetail(
        DomainServiceRequest serviceRequest,
        IReadOnlyCollection<ServiceChecklistMaster> checklistMasters,
        string lifecycleType,
        string lifecycleLabel,
        Coolzo.Application.Common.Models.SupportJobAlertView supportJobAlert)
    {
        var booking = serviceRequest.Booking;
        var primaryLine = booking?.BookingLines.OrderBy(line => line.BookingLineId).FirstOrDefault();
        var activeAssignment = GetActiveAssignment(serviceRequest);
        var slotDate = booking?.SlotAvailability?.SlotDate ?? DateOnly.FromDateTime(serviceRequest.ServiceRequestDateUtc);
        var slotLabel = booking?.SlotAvailability?.SlotConfiguration?.SlotLabel ?? "Preferred Slot";
        var quotation = serviceRequest.JobCard?.Quotations
            .Where(item => !item.IsDeleted)
            .OrderByDescending(item => item.QuotationDateUtc)
            .FirstOrDefault();

        return new TechnicianJobDetailResponse(
            serviceRequest.ServiceRequestId,
            serviceRequest.ServiceRequestNumber,
            lifecycleType,
            lifecycleLabel,
            serviceRequest.BookingId,
            booking?.BookingReference ?? string.Empty,
            serviceRequest.CurrentStatus.ToString(),
            booking?.CustomerNameSnapshot ?? string.Empty,
            booking?.MobileNumberSnapshot ?? string.Empty,
            BuildAddressSummary(booking),
            booking?.ZoneNameSnapshot ?? string.Empty,
            primaryLine?.Service?.ServiceName ?? booking?.ServiceNameSnapshot ?? string.Empty,
            primaryLine?.AcType?.AcTypeName ?? string.Empty,
            primaryLine?.Tonnage?.TonnageName ?? string.Empty,
            primaryLine?.Brand?.BrandName ?? string.Empty,
            primaryLine?.ModelName ?? string.Empty,
            primaryLine?.IssueNotes ?? string.Empty,
            slotDate,
            slotLabel,
            activeAssignment?.AssignmentRemarks,
            ToJobCardSummary(serviceRequest.JobCard),
            quotation?.QuotationHeaderId,
            quotation?.QuotationNumber,
            quotation?.CurrentStatus.ToString(),
            ToDiagnosisSummary(serviceRequest.JobCard),
            ToChecklistSummary(serviceRequest, checklistMasters),
            ToChecklistItems(serviceRequest, checklistMasters),
            ToExecutionNotes(serviceRequest.JobCard, false),
            ToAttachments(serviceRequest.JobCard),
            ToExecutionTimeline(serviceRequest),
            SupportTicketResponseMapper.ToJobAlertResponse(supportJobAlert),
            ToAllowedActions(serviceRequest.CurrentStatus));
    }

    public static JobCardSummaryResponse ToJobCardSummary(JobCard? jobCard)
    {
        return new JobCardSummaryResponse(
            jobCard?.JobCardId,
            jobCard?.JobCardNumber,
            jobCard?.WorkStartedDateUtc,
            jobCard?.WorkInProgressDateUtc,
            jobCard?.WorkCompletedDateUtc,
            jobCard?.SubmittedForClosureDateUtc,
            string.IsNullOrWhiteSpace(jobCard?.CompletionSummary) ? null : jobCard.CompletionSummary,
            jobCard?.ExecutionNotes.Count(note => !note.IsDeleted) ?? 0,
            jobCard?.Attachments.Count(attachment => !attachment.IsDeleted) ?? 0);
    }

    public static JobDiagnosisSummaryResponse ToDiagnosisSummary(JobCard? jobCard)
    {
        var diagnosis = jobCard?.JobDiagnosis;

        return new JobDiagnosisSummaryResponse(
            diagnosis?.JobDiagnosisId,
            diagnosis?.ComplaintIssueMasterId,
            diagnosis?.ComplaintIssueMaster?.IssueName,
            diagnosis?.DiagnosisResultMasterId,
            diagnosis?.DiagnosisResultMaster?.ResultName,
            string.IsNullOrWhiteSpace(diagnosis?.DiagnosisRemarks) ? null : diagnosis.DiagnosisRemarks,
            diagnosis?.DiagnosisDateUtc);
    }

    public static JobChecklistSummaryResponse ToChecklistSummary(
        DomainServiceRequest serviceRequest,
        IReadOnlyCollection<ServiceChecklistMaster> checklistMasters)
    {
        var responses = serviceRequest.JobCard?.ChecklistResponses
            .Where(response => !response.IsDeleted)
            .ToArray() ?? Array.Empty<JobChecklistResponse>();
        var totalItems = checklistMasters.Count;
        var respondedItems = responses.Count(response => response.IsChecked.HasValue || !string.IsNullOrWhiteSpace(response.ResponseRemarks));
        var mandatoryItems = checklistMasters.Count(master => master.IsMandatory);
        var mandatoryRespondedItems = checklistMasters.Count(master =>
        {
            var response = responses.FirstOrDefault(item => item.ServiceChecklistMasterId == master.ServiceChecklistMasterId);
            return response is not null && (response.IsChecked.HasValue || !string.IsNullOrWhiteSpace(response.ResponseRemarks));
        });

        return new JobChecklistSummaryResponse(totalItems, respondedItems, mandatoryItems, mandatoryRespondedItems);
    }

    public static IReadOnlyCollection<JobChecklistItemResponse> ToChecklistItems(
        DomainServiceRequest serviceRequest,
        IReadOnlyCollection<ServiceChecklistMaster> checklistMasters)
    {
        var responseLookup = serviceRequest.JobCard?.ChecklistResponses
            .Where(response => !response.IsDeleted)
            .ToDictionary(response => response.ServiceChecklistMasterId) ?? new Dictionary<long, JobChecklistResponse>();

        return checklistMasters
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.ChecklistTitle)
            .Select(master =>
            {
                responseLookup.TryGetValue(master.ServiceChecklistMasterId, out var response);

                return new JobChecklistItemResponse(
                    master.ServiceChecklistMasterId,
                    master.ChecklistTitle,
                    master.ChecklistDescription,
                    master.IsMandatory,
                    response?.IsChecked,
                    response?.ResponseRemarks ?? string.Empty,
                    response?.ResponseDateUtc);
            })
            .ToArray();
    }

    public static IReadOnlyCollection<JobExecutionNoteResponse> ToExecutionNotes(JobCard? jobCard, bool customerVisibleOnly)
    {
        IEnumerable<JobExecutionNote> query = jobCard?.ExecutionNotes.Where(note => !note.IsDeleted)
            ?? Array.Empty<JobExecutionNote>();

        if (customerVisibleOnly)
        {
            query = query.Where(note => note.IsCustomerVisible);
        }

        return query
            .OrderByDescending(note => note.NoteDateUtc)
            .Select(note => new JobExecutionNoteResponse(
                note.JobExecutionNoteId,
                note.NoteText,
                note.IsCustomerVisible,
                note.CreatedBy,
                note.NoteDateUtc))
            .ToArray();
    }

    public static IReadOnlyCollection<JobAttachmentResponse> ToAttachments(JobCard? jobCard)
    {
        return jobCard?.Attachments
            .Where(attachment => !attachment.IsDeleted)
            .OrderByDescending(attachment => attachment.UploadedDateUtc)
            .Select(attachment => new JobAttachmentResponse(
                attachment.JobAttachmentId,
                attachment.AttachmentType.ToString(),
                attachment.FileName,
                attachment.ContentType,
                attachment.FileSizeInBytes,
                attachment.RelativePath,
                attachment.AttachmentRemarks,
                attachment.UploadedDateUtc))
            .ToArray() ?? Array.Empty<JobAttachmentResponse>();
    }

    public static IReadOnlyCollection<JobExecutionTimelineItemResponse> ToExecutionTimeline(DomainServiceRequest serviceRequest)
    {
        if (serviceRequest.JobCard is not null && serviceRequest.JobCard.ExecutionTimelines.Any(item => !item.IsDeleted))
        {
            return serviceRequest.JobCard.ExecutionTimelines
                .Where(item => !item.IsDeleted)
                .OrderBy(item => item.EventDateUtc)
                .Select(item => new JobExecutionTimelineItemResponse(
                    item.EventType,
                    item.EventTitle,
                    item.Status.ToString(),
                    item.Remarks,
                    item.EventDateUtc))
                .ToArray();
        }

        return serviceRequest.StatusHistories
            .Where(history => !history.IsDeleted && history.Status != ServiceRequestStatus.New)
            .OrderBy(history => history.StatusDateUtc)
            .Select(history => new JobExecutionTimelineItemResponse(
                "StatusChanged",
                history.Status.ToString(),
                history.Status.ToString(),
                history.Remarks,
                history.StatusDateUtc))
            .ToArray();
    }

    public static IReadOnlyCollection<string> ToAllowedActions(ServiceRequestStatus currentStatus)
    {
        return currentStatus switch
        {
            ServiceRequestStatus.Assigned => new[] { "mark-enroute" },
            ServiceRequestStatus.EnRoute => new[] { "mark-reached" },
            ServiceRequestStatus.Reached => new[] { "start-work" },
            ServiceRequestStatus.WorkStarted => new[] { "mark-in-progress" },
            ServiceRequestStatus.WorkInProgress => new[] { "mark-work-completed" },
            ServiceRequestStatus.WorkCompletedPendingSubmission => new[] { "submit-for-closure" },
            _ => Array.Empty<string>()
        };
    }

    public static string BuildAddressSummary(DomainBooking? booking)
    {
        if (booking is null)
        {
            return string.Empty;
        }

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

    public static ServiceRequestAssignment? GetActiveAssignment(DomainServiceRequest serviceRequest)
    {
        return serviceRequest.Assignments
            .Where(assignment => assignment.IsActiveAssignment && !assignment.IsDeleted)
            .OrderByDescending(assignment => assignment.AssignedDateUtc)
            .FirstOrDefault();
    }
}
