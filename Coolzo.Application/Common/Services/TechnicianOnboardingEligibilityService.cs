using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;

namespace Coolzo.Application.Common.Services;

public sealed class TechnicianOnboardingEligibilityService
{
    private static readonly string[] RequiredDocumentTypes =
    [
        "IdentityProof",
        "AddressProof",
        "TechnicalCertificate"
    ];

    public TechnicianOnboardingEligibilityResult Evaluate(
        Technician technician,
        IReadOnlyCollection<TechnicianDocument> documents,
        IReadOnlyCollection<SkillAssessment> skillAssessments,
        IReadOnlyCollection<TrainingRecord> trainingRecords)
    {
        var pendingItems = new List<string>();
        var normalizedDocumentTypes = documents
            .Where(document => !string.IsNullOrWhiteSpace(document.DocumentType))
            .ToDictionary(
                document => document.DocumentType.Trim().ToUpperInvariant(),
                document => document,
                StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(technician.TechnicianName) || string.IsNullOrWhiteSpace(technician.MobileNumber))
        {
            pendingItems.Add("Technician profile is incomplete.");
        }

        foreach (var documentType in RequiredDocumentTypes)
        {
            if (!normalizedDocumentTypes.TryGetValue(documentType.ToUpperInvariant(), out var document))
            {
                pendingItems.Add($"{documentType} is required.");
                continue;
            }

            if (document.VerificationStatus != TechnicianDocumentStatus.Verified)
            {
                pendingItems.Add($"{documentType} must be verified.");
            }
        }

        if (!skillAssessments.Any(assessment => assessment.AssessmentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) && assessment.PassFlag))
        {
            pendingItems.Add("At least one completed and passed skill assessment is required.");
        }

        if (!trainingRecords.Any(record => record.IsCompleted || record.TrainingStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase)))
        {
            pendingItems.Add("At least one completed training record is required.");
        }

        var status = technician.IsActive
            ? "Active"
            : pendingItems.Count == 0
                ? "ReadyForActivation"
                : documents.Count == 0
                    ? "DocumentsPending"
                    : documents.Any(document => document.VerificationStatus != TechnicianDocumentStatus.Verified)
                        ? "VerificationPending"
                        : !skillAssessments.Any()
                            ? "AssessmentPending"
                            : !trainingRecords.Any()
                                ? "TrainingPending"
                                : "Draft";

        return new TechnicianOnboardingEligibilityResult(pendingItems.Count == 0 && !technician.IsActive, status, pendingItems);
    }
}

public sealed record TechnicianOnboardingEligibilityResult(
    bool IsEligible,
    string OnboardingStatus,
    IReadOnlyCollection<string> PendingItems);
