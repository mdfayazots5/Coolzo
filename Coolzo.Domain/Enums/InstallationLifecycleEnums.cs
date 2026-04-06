namespace Coolzo.Domain.Enums;

public enum InstallationLifecycleStatus
{
    LeadCreated = 1,
    SurveyScheduled = 2,
    SurveyCompleted = 3,
    ProposalGenerated = 4,
    ProposalApproved = 5,
    ProposalRejected = 6,
    InstallationScheduled = 7,
    InstallationInProgress = 8,
    InstallationCompleted = 9,
    Commissioned = 10,
    Cancelled = 11
}

public enum InstallationApprovalStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum InstallationProposalStatus
{
    Draft = 1,
    PendingApproval = 2,
    Approved = 3,
    Rejected = 4
}
