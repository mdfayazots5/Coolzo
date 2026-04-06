namespace Coolzo.Contracts.Requests.FieldExecution;

public sealed record SaveJobDiagnosisRequest(
    long? ComplaintIssueMasterId,
    long? DiagnosisResultMasterId,
    string? DiagnosisRemarks);
