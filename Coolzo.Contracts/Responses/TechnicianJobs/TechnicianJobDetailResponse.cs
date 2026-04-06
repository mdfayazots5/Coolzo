using Coolzo.Contracts.Responses.FieldExecution;
using Coolzo.Contracts.Responses.Support;

namespace Coolzo.Contracts.Responses.TechnicianJobs;

public sealed record TechnicianJobDetailResponse(
    long ServiceRequestId,
    string ServiceRequestNumber,
    string LifecycleType,
    string LifecycleLabel,
    long BookingId,
    string BookingReference,
    string CurrentStatus,
    string CustomerName,
    string MobileNumber,
    string AddressSummary,
    string ZoneName,
    string ServiceName,
    string AcTypeName,
    string TonnageName,
    string BrandName,
    string ModelName,
    string IssueNotes,
    DateOnly SlotDate,
    string SlotLabel,
    string? AssignmentRemarks,
    JobCardSummaryResponse JobCard,
    long? QuotationId,
    string? QuotationNumber,
    string? QuotationStatus,
    JobDiagnosisSummaryResponse Diagnosis,
    JobChecklistSummaryResponse ChecklistSummary,
    IReadOnlyCollection<JobChecklistItemResponse> ChecklistItems,
    IReadOnlyCollection<JobExecutionNoteResponse> Notes,
    IReadOnlyCollection<JobAttachmentResponse> Attachments,
    IReadOnlyCollection<JobExecutionTimelineItemResponse> Timeline,
    SupportTicketJobAlertResponse SupportAlert,
    IReadOnlyCollection<string> AllowedActions);
