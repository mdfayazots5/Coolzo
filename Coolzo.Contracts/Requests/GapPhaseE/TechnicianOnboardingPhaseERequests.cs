namespace Coolzo.Contracts.Requests.GapPhaseE;

public sealed record VerifyTechnicianDocumentRequest(
    string? Remarks);

public sealed record RejectTechnicianDocumentRequest(
    string Remarks);

public sealed record CreateSkillAssessmentRequest(
    long? SkillTagId,
    string AssessmentCode,
    string AssessmentName,
    string? Remarks);

public sealed record SubmitSkillAssessmentResultRequest(
    decimal ScorePercentage,
    bool PassFlag,
    string? Remarks);

public sealed record CreateTrainingRecordRequest(
    string TrainingTitle,
    string TrainingType,
    string? Remarks);

public sealed record CompleteTrainingRecordRequest(
    string? CertificationNumber,
    decimal? ScorePercentage,
    string? CertificateUrl,
    string? Remarks);

public sealed record ActivateTechnicianPhaseERequest(
    string ActivationReason);

public sealed record DeactivateTechnicianRequest(
    string ActivationReason);
