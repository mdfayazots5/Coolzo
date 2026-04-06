namespace Coolzo.Contracts.Requests.GapPhaseC;

public sealed record CreateInstallationRequest(
    long? LeadId,
    string CustomerName,
    string MobileNumber,
    string? EmailAddress,
    string SourceChannel,
    string AddressLine1,
    string? AddressLine2,
    string CityName,
    string Pincode,
    string InstallationType,
    int NumberOfUnits,
    string? SiteNotes,
    DateTime? PreferredSurveyDateUtc);

public sealed record ScheduleInstallationSurveyRequest(
    DateTime SurveyDateUtc,
    long? TechnicianId,
    string? Remarks);

public sealed record InstallationSurveyItemRequest(
    string ItemTitle,
    string? ItemValue,
    string? Unit,
    string? Remarks,
    bool IsMandatory);

public sealed record SubmitInstallationSurveyRequest(
    string SiteConditionSummary,
    bool ElectricalReadiness,
    bool AccessReadiness,
    string? SafetyRiskNotes,
    string? RecommendedAction,
    decimal EstimatedMaterialCost,
    string? MeasurementsJson,
    string? PhotoUrlsJson,
    IReadOnlyCollection<InstallationSurveyItemRequest> Items);

public sealed record InstallationProposalLineRequest(
    string LineDescription,
    int Quantity,
    decimal UnitPrice,
    string? Remarks);

public sealed record CreateInstallationProposalRequest(
    string? ProposalRemarks,
    IReadOnlyCollection<InstallationProposalLineRequest> Lines);

public sealed record ApproveInstallationProposalRequest(
    string? CustomerRemarks);

public sealed record RejectInstallationProposalRequest(
    string? CustomerRemarks);

public sealed record CreateInstallationExecutionOrderRequest(
    long? TechnicianId,
    DateTime? ScheduledInstallationDateUtc,
    int HelperCount,
    string? ExecutionRemarks);

public sealed record StartInstallationRequest(
    string? Remarks);

public sealed record CompleteInstallationRequest(
    string WorkSummary);

public sealed record InstallationChecklistItemRequest(
    string ChecklistTitle,
    string? ChecklistDescription,
    bool IsMandatory,
    bool IsCompleted,
    string? ResponseRemarks);

public sealed record SaveInstallationChecklistRequest(
    IReadOnlyCollection<InstallationChecklistItemRequest> Items);

public sealed record GenerateInstallationCommissioningRequest(
    string CustomerConfirmationName,
    string CustomerSignatureName,
    string? ChecklistJson,
    string? Remarks,
    bool IsAccepted);
