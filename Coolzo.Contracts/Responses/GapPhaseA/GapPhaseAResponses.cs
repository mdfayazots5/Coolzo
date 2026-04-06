namespace Coolzo.Contracts.Responses.GapPhaseA;

public sealed record LeadResponse(
    long LeadId,
    string LeadNumber,
    string CustomerName,
    string MobileNumber,
    string LeadStatus,
    long? ConvertedBookingId,
    long? ConvertedServiceRequestId);

public sealed record LeadListItemResponse(
    long LeadId,
    string LeadNumber,
    string CustomerName,
    string MobileNumber,
    string? EmailAddress,
    string SourceChannel,
    string LeadStatus,
    long? AssignedUserId,
    string? AssignedUserName,
    DateTime DateCreated,
    DateTime? LastContactedDateUtc,
    DateTime? ConvertedDateUtc,
    string? LostReason);

public sealed record LeadStatusHistoryResponse(
    long LeadStatusHistoryId,
    string PreviousStatus,
    string CurrentStatus,
    string Remarks,
    string CreatedBy,
    DateTime ChangedDateUtc);

public sealed record LeadAssignmentResponse(
    long LeadAssignmentId,
    long AssignedUserId,
    string AssignedUserName,
    long? PreviousAssignedUserId,
    string Remarks,
    string CreatedBy,
    DateTime AssignedDateUtc);

public sealed record LeadNoteResponse(
    long LeadNoteId,
    string NoteText,
    bool IsInternal,
    string CreatedBy,
    DateTime NoteDateUtc);

public sealed record LeadConversionResponse(
    long LeadConversionId,
    string ConversionType,
    long? BookingId,
    long? ServiceRequestId,
    string ReferenceNumber,
    string Remarks,
    DateTime ConvertedDateUtc);

public sealed record LeadDetailResponse(
    long LeadId,
    string LeadNumber,
    string CustomerName,
    string MobileNumber,
    string? EmailAddress,
    string SourceChannel,
    string LeadStatus,
    long? AssignedUserId,
    string? AssignedUserName,
    long? ServiceId,
    long? AcTypeId,
    long? TonnageId,
    long? BrandId,
    long? SlotAvailabilityId,
    long? ConvertedBookingId,
    long? ConvertedServiceRequestId,
    string? AddressLine1,
    string? AddressLine2,
    string? CityName,
    string? Pincode,
    string? InquiryNotes,
    string? LostReason,
    DateTime DateCreated,
    DateTime? LastContactedDateUtc,
    DateTime? ConvertedDateUtc,
    DateTime? ClosedDateUtc,
    IReadOnlyCollection<LeadStatusHistoryResponse> StatusTimeline,
    IReadOnlyCollection<LeadAssignmentResponse> Assignments,
    IReadOnlyCollection<LeadNoteResponse> Notes,
    IReadOnlyCollection<LeadConversionResponse> Conversions);

public sealed record LeadSourceAnalyticsResponse(
    string SourceChannel,
    int LeadCount,
    int ConvertedCount);

public sealed record LeadDailyCountResponse(
    DateOnly Date,
    int LeadCount);

public sealed record LeadAnalyticsResponse(
    DateOnly FromDate,
    DateOnly ToDate,
    int TotalLeads,
    int ContactedLeads,
    int QualifiedLeads,
    int ConvertedLeads,
    int LostLeads,
    int ClosedLeads,
    decimal ConversionRate,
    IReadOnlyCollection<LeadSourceAnalyticsResponse> LeadsBySource,
    IReadOnlyCollection<LeadDailyCountResponse> DailyLeadCount);

public sealed record InstallationOrderResponse(
    long InstallationOrderId,
    string InstallationOrderNumber,
    string CurrentStatus,
    long CustomerId,
    long? ServiceRequestId,
    DateTime? ScheduledInstallationDateUtc,
    int SurveyReportCount);

public sealed record CommissioningCertificateResponse(
    long CommissioningCertificateId,
    long InstallationOrderId,
    string CertificateNumber,
    DateTime CommissioningDateUtc,
    bool IsAccepted);

public sealed record CancellationRecordResponse(
    long CancellationRecordId,
    long ServiceRequestId,
    string CancellationStatus,
    decimal CancellationFeeAmount,
    decimal RefundEligibleAmount,
    bool RequiresApproval);

public sealed record RefundRequestResponse(
    long RefundRequestId,
    long CancellationRecordId,
    long InvoiceId,
    string RefundStatus,
    decimal RequestedAmount,
    decimal ApprovedAmount);

public sealed record TechnicianOnboardingResponse(
    long TechnicianId,
    string TechnicianCode,
    string TechnicianName,
    bool IsActive,
    int DocumentCount,
    string LatestAssessmentResult,
    int CompletedTrainingCount);

public sealed record EscalationResponse(
    long SystemAlertId,
    string AlertCode,
    string AlertType,
    string RelatedEntityName,
    string RelatedEntityId,
    string Severity,
    string AlertStatus,
    int EscalationLevel);

public sealed record CampaignResponse(
    long CampaignId,
    string CampaignCode,
    string CampaignName,
    string CampaignStatus,
    int PlannedBookingCount,
    int AllocatedBookingCount,
    long SlotAvailabilityId);

public sealed record PartsReturnResponse(
    long PartsReturnId,
    string PartsReturnNumber,
    string PartsReturnStatus,
    decimal Quantity,
    string SupplierClaimReference);

public sealed record SupplierClaimResponse(
    long PartsReturnId,
    string PartsReturnNumber,
    string PartsReturnStatus,
    string SupplierClaimReference);

public sealed record SystemHealthResponse(
    string Status,
    int OpenAlertCount,
    int PendingOfflineSyncCount,
    int PendingWebhookRetryCount,
    int EnabledFeatureFlagCount,
    IReadOnlyCollection<string> CriticalTriggerCodes);
