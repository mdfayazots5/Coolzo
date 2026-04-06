namespace Coolzo.Contracts.Responses.FieldExecution;

public sealed record JobDiagnosisSummaryResponse(
    long? JobDiagnosisId,
    long? ComplaintIssueMasterId,
    string? ComplaintIssueName,
    long? DiagnosisResultMasterId,
    string? DiagnosisResultName,
    string? DiagnosisRemarks,
    DateTime? DiagnosisDateUtc);
