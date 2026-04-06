namespace Coolzo.Contracts.Requests.GapPhaseA;

public sealed record CreateTechnicianDraftRequest(
    string TechnicianName,
    string MobileNumber,
    string? EmailAddress,
    long? BaseZoneId,
    int MaxDailyAssignments);

public sealed record TechnicianDocumentInput(
    string DocumentType,
    string? DocumentNumber,
    string? DocumentUrl,
    DateTime? ExpiryDateUtc);

public sealed record UploadTechnicianDocumentsRequest(
    IReadOnlyCollection<TechnicianDocumentInput> Documents);

public sealed record ActivateTechnicianRequest(
    string AssessmentCode,
    decimal ScorePercentage,
    string TrainingName,
    string? CertificationNumber,
    decimal TrainingScorePercentage,
    string? Remarks);
