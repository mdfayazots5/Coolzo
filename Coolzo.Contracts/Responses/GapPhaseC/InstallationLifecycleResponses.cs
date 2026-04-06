namespace Coolzo.Contracts.Responses.GapPhaseC;

public sealed record InstallationSummaryResponse(
    long InstallationId,
    string InstallationNumber,
    long? LeadId,
    string InstallationStatus,
    string ApprovalStatus,
    string CustomerName,
    string MobileNumber,
    string InstallationType,
    int NumberOfUnits,
    DateTime? SurveyDateUtc,
    DateTime? ScheduledInstallationDateUtc,
    string? ProposalNumber,
    decimal? ProposalTotalAmount,
    string? InstallationOrderNumber,
    string? WarrantyRegistrationNumber);

public sealed record InstallationListItemResponse(
    long InstallationId,
    string InstallationNumber,
    string CustomerName,
    string MobileNumber,
    string AddressSummary,
    string InstallationType,
    int NumberOfUnits,
    string InstallationStatus,
    string ApprovalStatus,
    DateTime? SurveyDateUtc,
    DateTime? ScheduledInstallationDateUtc,
    string? AssignedTechnicianName,
    string? ProposalNumber,
    decimal? ProposalTotalAmount,
    string? InstallationOrderNumber);

public sealed record InstallationSurveyItemResponse(
    long InstallationSurveyItemId,
    string ItemTitle,
    string? ItemValue,
    string? Unit,
    string Remarks,
    bool IsMandatory);

public sealed record InstallationSurveyResponse(
    long InstallationSurveyId,
    DateTime SurveyDateUtc,
    DateTime? CompletedDateUtc,
    long? TechnicianId,
    string? TechnicianName,
    string SiteConditionSummary,
    bool ElectricalReadiness,
    bool AccessReadiness,
    string SafetyRiskNotes,
    string RecommendedAction,
    decimal EstimatedMaterialCost,
    string MeasurementsJson,
    string PhotoUrlsJson,
    IReadOnlyCollection<InstallationSurveyItemResponse> Items);

public sealed record InstallationProposalLineResponse(
    long InstallationProposalLineId,
    string LineDescription,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    string Remarks);

public sealed record InstallationProposalResponse(
    long InstallationProposalId,
    string ProposalNumber,
    string ProposalStatus,
    decimal SubTotalAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    string ProposalRemarks,
    string CustomerRemarks,
    DateTime GeneratedDateUtc,
    DateTime? DecisionDateUtc,
    IReadOnlyCollection<InstallationProposalLineResponse> Lines);

public sealed record InstallationChecklistItemResponse(
    long InstallationChecklistId,
    string ChecklistTitle,
    string ChecklistDescription,
    bool IsMandatory,
    bool IsCompleted,
    string ResponseRemarks,
    DateTime? ResponseDateUtc);

public sealed record InstallationExecutionOrderResponse(
    long InstallationOrderId,
    string InstallationOrderNumber,
    string CurrentStatus,
    DateTime? ScheduledInstallationDateUtc,
    DateTime? ExecutionStartedDateUtc,
    DateTime? ExecutionCompletedDateUtc,
    long? TechnicianId,
    string? TechnicianName,
    int HelperCount);

public sealed record InstallationCommissioningResponse(
    long CommissioningCertificateId,
    string CertificateNumber,
    string WarrantyRegistrationNumber,
    DateTime CommissioningDateUtc,
    string CustomerConfirmationName,
    string CustomerSignatureName,
    bool IsAccepted,
    string Remarks);

public sealed record InstallationStatusHistoryResponse(
    long InstallationStatusHistoryId,
    string PreviousStatus,
    string CurrentStatus,
    string Remarks,
    string ChangedByRole,
    string CreatedBy,
    DateTime ChangedDateUtc);

public sealed record InstallationDetailResponse(
    long InstallationId,
    string InstallationNumber,
    long? LeadId,
    string? LeadNumber,
    long CustomerId,
    string CustomerName,
    string MobileNumber,
    string? EmailAddress,
    long CustomerAddressId,
    string AddressLine1,
    string AddressLine2,
    string CityName,
    string Pincode,
    string InstallationType,
    int NumberOfUnits,
    string SiteNotes,
    string InstallationStatus,
    string ApprovalStatus,
    long? AssignedTechnicianId,
    string? AssignedTechnicianName,
    DateTime? SurveyDateUtc,
    DateTime? ProposalApprovedDateUtc,
    DateTime? ScheduledInstallationDateUtc,
    DateTime? InstallationStartedDateUtc,
    DateTime? InstallationCompletedDateUtc,
    DateTime? CommissionedDateUtc,
    IReadOnlyCollection<InstallationSurveyResponse> Surveys,
    IReadOnlyCollection<InstallationProposalResponse> Proposals,
    IReadOnlyCollection<InstallationChecklistItemResponse> ChecklistItems,
    IReadOnlyCollection<InstallationExecutionOrderResponse> Orders,
    IReadOnlyCollection<InstallationCommissioningResponse> CommissioningCertificates,
    IReadOnlyCollection<InstallationStatusHistoryResponse> StatusTimeline);
