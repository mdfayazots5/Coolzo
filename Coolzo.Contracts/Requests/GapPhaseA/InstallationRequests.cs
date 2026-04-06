namespace Coolzo.Contracts.Requests.GapPhaseA;

public sealed record CreateInstallationOrderRequest(
    long? LeadId,
    long? ServiceRequestId,
    long CustomerId,
    long CustomerAddressId,
    long? TechnicianId,
    DateTime? ScheduledInstallationDateUtc,
    string? InstallationChecklistJson);

public sealed record SubmitSurveyReportRequest(
    string SurveyDecision,
    string SiteConditionSummary,
    bool ElectricalReadiness,
    bool AccessReadiness,
    string? SafetyRiskNotes,
    string? RecommendedAction,
    decimal EstimatedMaterialCost,
    string? SyncDeviceReference,
    string? SyncReference);

public sealed record CreateCommissioningCertificateRequest(
    string CustomerConfirmationName,
    string? ChecklistJson,
    string? Remarks,
    bool IsAccepted);
