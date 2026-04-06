using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;

namespace Coolzo.Application.Common.Services;

public sealed class InstallationLifecycleWorkflowService
{
    public void EnsureTransition(
        InstallationLead installation,
        InstallationLifecycleStatus targetStatus,
        string remarks,
        string actorName,
        string actorRole,
        string ipAddress,
        DateTime now)
    {
        if (!CanTransition(installation.InstallationStatus, targetStatus))
        {
            throw new AppException(ErrorCodes.InvalidStatusTransition, "The requested installation status transition is not allowed.", 409);
        }

        installation.StatusHistories.Add(new InstallationStatusHistory
        {
            PreviousStatus = installation.InstallationStatus,
            CurrentStatus = targetStatus,
            Remarks = remarks,
            ChangedByRole = actorRole,
            ChangedDateUtc = now,
            CreatedBy = actorName,
            DateCreated = now,
            IPAddress = ipAddress
        });

        installation.InstallationStatus = targetStatus;
        installation.UpdatedBy = actorName;
        installation.LastUpdated = now;
    }

    private static bool CanTransition(InstallationLifecycleStatus currentStatus, InstallationLifecycleStatus targetStatus)
    {
        if (currentStatus == targetStatus)
        {
            return true;
        }

        return currentStatus switch
        {
            InstallationLifecycleStatus.LeadCreated => targetStatus is InstallationLifecycleStatus.SurveyScheduled or InstallationLifecycleStatus.Cancelled,
            InstallationLifecycleStatus.SurveyScheduled => targetStatus is InstallationLifecycleStatus.SurveyCompleted or InstallationLifecycleStatus.Cancelled,
            InstallationLifecycleStatus.SurveyCompleted => targetStatus is InstallationLifecycleStatus.ProposalGenerated or InstallationLifecycleStatus.Cancelled,
            InstallationLifecycleStatus.ProposalGenerated => targetStatus is InstallationLifecycleStatus.ProposalApproved or InstallationLifecycleStatus.ProposalRejected or InstallationLifecycleStatus.Cancelled,
            InstallationLifecycleStatus.ProposalRejected => targetStatus is InstallationLifecycleStatus.ProposalGenerated or InstallationLifecycleStatus.Cancelled,
            InstallationLifecycleStatus.ProposalApproved => targetStatus is InstallationLifecycleStatus.InstallationScheduled,
            InstallationLifecycleStatus.InstallationScheduled => targetStatus is InstallationLifecycleStatus.InstallationInProgress or InstallationLifecycleStatus.Cancelled,
            InstallationLifecycleStatus.InstallationInProgress => targetStatus is InstallationLifecycleStatus.InstallationCompleted or InstallationLifecycleStatus.Cancelled,
            InstallationLifecycleStatus.InstallationCompleted => targetStatus == InstallationLifecycleStatus.Commissioned,
            _ => false
        };
    }
}
