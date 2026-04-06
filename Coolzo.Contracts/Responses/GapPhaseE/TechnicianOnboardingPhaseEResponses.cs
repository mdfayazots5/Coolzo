namespace Coolzo.Contracts.Responses.GapPhaseE;

public sealed record TechnicianOnboardingListItemResponse(
    long TechnicianId,
    string TechnicianCode,
    string TechnicianName,
    string MobileNumber,
    string EmailAddress,
    bool IsActive,
    string OnboardingStatus,
    int UploadedDocumentCount,
    int VerifiedDocumentCount,
    string LatestAssessmentResult,
    int CompletedTrainingCount,
    bool IsActivationEligible);

public sealed record TechnicianDocumentDetailResponse(
    long TechnicianDocumentId,
    string DocumentType,
    string DocumentNumber,
    string StorageUrl,
    string VerificationStatus,
    string VerificationRemarks,
    DateTime? ExpiryDateUtc,
    long? VerifiedByUserId,
    DateTime? VerifiedOnUtc);

public sealed record SkillAssessmentDetailResponse(
    long SkillAssessmentId,
    long? SkillTagId,
    string AssessmentCode,
    string AssessmentName,
    string AssessmentStatus,
    decimal ScorePercentage,
    string AssessmentResult,
    bool PassFlag,
    long? AssessedByUserId,
    DateTime? AssessedOnUtc,
    string Remarks);

public sealed record TrainingRecordDetailResponse(
    long TrainingRecordId,
    string TrainingTitle,
    string TrainingType,
    string TrainingStatus,
    string CertificationNumber,
    decimal ScorePercentage,
    bool IsCompleted,
    DateTime? TrainingCompletionDateUtc,
    long? TrainerUserId,
    string CertificateUrl,
    string Remarks);

public sealed record TechnicianActivationLogResponse(
    long TechnicianActivationLogId,
    string ActivationAction,
    string ActivationReason,
    long? ActivatedByUserId,
    DateTime ActivatedOnUtc,
    string EligibilitySnapshot);

public sealed record TechnicianOnboardingDetailResponse(
    long TechnicianId,
    string TechnicianCode,
    string TechnicianName,
    string MobileNumber,
    string EmailAddress,
    long? BaseZoneId,
    int MaxDailyAssignments,
    bool IsActive,
    string OnboardingStatus,
    bool IsActivationEligible,
    IReadOnlyCollection<string> PendingEligibilityItems,
    IReadOnlyCollection<TechnicianDocumentDetailResponse> Documents,
    IReadOnlyCollection<SkillAssessmentDetailResponse> SkillAssessments,
    IReadOnlyCollection<TrainingRecordDetailResponse> TrainingRecords,
    IReadOnlyCollection<TechnicianActivationLogResponse> ActivationHistory);
