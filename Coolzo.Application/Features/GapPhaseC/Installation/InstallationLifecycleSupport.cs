using System.Text.Json;
using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.GapPhaseC;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;

namespace Coolzo.Application.Features.GapPhaseC.Installation;

internal static class InstallationLifecycleSupport
{
    public static string ResolveActorName(ICurrentUserContext currentUserContext, string fallbackActor)
    {
        return string.IsNullOrWhiteSpace(currentUserContext.UserName) ? fallbackActor : currentUserContext.UserName;
    }

    public static string ResolveActorRole(ICurrentUserContext currentUserContext)
    {
        return currentUserContext.Roles.FirstOrDefault() ?? "System";
    }

    public static void ParseLeadSourceChannel(string sourceChannel, out LeadSourceChannel parsedSourceChannel)
    {
        parsedSourceChannel = sourceChannel.Trim().ToLowerInvariant() switch
        {
            "website" or "web" => LeadSourceChannel.Web,
            "phone" => LeadSourceChannel.Phone,
            "whatsapp" => LeadSourceChannel.WhatsApp,
            "app" or "mobileapp" or "mobile" => LeadSourceChannel.MobileApp,
            "manual" => LeadSourceChannel.Manual,
            _ => LeadSourceChannel.Web
        };
    }

    public static InstallationSummaryResponse MapSummary(InstallationLead installation)
    {
        var latestProposal = installation.Proposals
            .Where(proposal => !proposal.IsDeleted)
            .OrderByDescending(proposal => proposal.GeneratedDateUtc)
            .FirstOrDefault();

        var latestOrder = installation.Orders
            .Where(order => !order.IsDeleted)
            .OrderByDescending(order => order.DateCreated)
            .FirstOrDefault();

        var latestCommissioning = installation.CommissioningCertificates
            .Where(certificate => !certificate.IsDeleted)
            .OrderByDescending(certificate => certificate.CommissioningDateUtc)
            .FirstOrDefault();

        return new InstallationSummaryResponse(
            installation.InstallationId,
            installation.InstallationNumber,
            installation.LeadId,
            installation.InstallationStatus.ToString(),
            installation.ApprovalStatus.ToString(),
            installation.Customer?.CustomerName ?? string.Empty,
            installation.Customer?.MobileNumber ?? string.Empty,
            installation.InstallationType,
            installation.NumberOfUnits,
            installation.SurveyDateUtc,
            installation.ScheduledInstallationDateUtc,
            latestProposal?.ProposalNumber,
            latestProposal?.TotalAmount,
            latestOrder?.InstallationOrderNumber,
            latestCommissioning?.WarrantyRegistrationNumber);
    }

    public static InstallationListItemResponse MapListItem(InstallationLead installation)
    {
        var latestProposal = installation.Proposals
            .Where(proposal => !proposal.IsDeleted)
            .OrderByDescending(proposal => proposal.GeneratedDateUtc)
            .FirstOrDefault();

        var latestOrder = installation.Orders
            .Where(order => !order.IsDeleted)
            .OrderByDescending(order => order.DateCreated)
            .FirstOrDefault();

        var address = installation.CustomerAddress;
        var addressSummary = address is null
            ? string.Empty
            : string.Join(", ", new[]
            {
                address.AddressLine1,
                address.CityName,
                address.Pincode
            }.Where(value => !string.IsNullOrWhiteSpace(value)));

        return new InstallationListItemResponse(
            installation.InstallationId,
            installation.InstallationNumber,
            installation.Customer?.CustomerName ?? string.Empty,
            installation.Customer?.MobileNumber ?? string.Empty,
            addressSummary,
            installation.InstallationType,
            installation.NumberOfUnits,
            installation.InstallationStatus.ToString(),
            installation.ApprovalStatus.ToString(),
            installation.SurveyDateUtc,
            installation.ScheduledInstallationDateUtc,
            installation.AssignedTechnician?.TechnicianName,
            latestProposal?.ProposalNumber,
            latestProposal?.TotalAmount,
            latestOrder?.InstallationOrderNumber);
    }

    public static InstallationDetailResponse MapDetail(InstallationLead installation)
    {
        return new InstallationDetailResponse(
            installation.InstallationId,
            installation.InstallationNumber,
            installation.LeadId,
            installation.Lead?.LeadNumber,
            installation.CustomerId,
            installation.Customer?.CustomerName ?? string.Empty,
            installation.Customer?.MobileNumber ?? string.Empty,
            string.IsNullOrWhiteSpace(installation.Customer?.EmailAddress) ? null : installation.Customer.EmailAddress,
            installation.CustomerAddressId,
            installation.CustomerAddress?.AddressLine1 ?? string.Empty,
            installation.CustomerAddress?.AddressLine2 ?? string.Empty,
            installation.CustomerAddress?.CityName ?? string.Empty,
            installation.CustomerAddress?.Pincode ?? string.Empty,
            installation.InstallationType,
            installation.NumberOfUnits,
            installation.SiteNotes,
            installation.InstallationStatus.ToString(),
            installation.ApprovalStatus.ToString(),
            installation.AssignedTechnicianId,
            installation.AssignedTechnician?.TechnicianName,
            installation.SurveyDateUtc,
            installation.ProposalApprovedDateUtc,
            installation.ScheduledInstallationDateUtc,
            installation.InstallationStartedDateUtc,
            installation.InstallationCompletedDateUtc,
            installation.CommissionedDateUtc,
            installation.Surveys
                .Where(survey => !survey.IsDeleted)
                .OrderByDescending(survey => survey.SurveyDateUtc)
                .Select(MapSurvey)
                .ToArray(),
            installation.Proposals
                .Where(proposal => !proposal.IsDeleted)
                .OrderByDescending(proposal => proposal.GeneratedDateUtc)
                .Select(MapProposal)
                .ToArray(),
            installation.Checklists
                .Where(checklist => !checklist.IsDeleted)
                .OrderBy(checklist => checklist.SortOrder)
                .ThenBy(checklist => checklist.InstallationChecklistId)
                .Select(MapChecklist)
                .ToArray(),
            installation.Orders
                .Where(order => !order.IsDeleted)
                .OrderByDescending(order => order.DateCreated)
                .Select(MapOrder)
                .ToArray(),
            installation.CommissioningCertificates
                .Where(certificate => !certificate.IsDeleted)
                .OrderByDescending(certificate => certificate.CommissioningDateUtc)
                .Select(MapCommissioning)
                .ToArray(),
            installation.StatusHistories
                .Where(history => !history.IsDeleted)
                .OrderByDescending(history => history.ChangedDateUtc)
                .Select(MapStatusHistory)
                .ToArray());
    }

    public static InstallationSurveyResponse MapSurvey(InstallationSurvey survey)
    {
        return new InstallationSurveyResponse(
            survey.InstallationSurveyId,
            survey.SurveyDateUtc,
            survey.CompletedDateUtc,
            survey.TechnicianId,
            survey.Technician?.TechnicianName,
            survey.SiteConditionSummary,
            survey.ElectricalReadiness,
            survey.AccessReadiness,
            survey.SafetyRiskNotes,
            survey.RecommendedAction,
            survey.EstimatedMaterialCost,
            survey.MeasurementsJson,
            survey.PhotoUrlsJson,
            survey.Items
                .Where(item => !item.IsDeleted)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.InstallationSurveyItemId)
                .Select(item => new InstallationSurveyItemResponse(
                    item.InstallationSurveyItemId,
                    item.ItemTitle,
                    string.IsNullOrWhiteSpace(item.ItemValue) ? null : item.ItemValue,
                    string.IsNullOrWhiteSpace(item.Unit) ? null : item.Unit,
                    item.Remarks,
                    item.IsMandatory))
                .ToArray());
    }

    public static InstallationProposalResponse MapProposal(InstallationProposal proposal)
    {
        return new InstallationProposalResponse(
            proposal.InstallationProposalId,
            proposal.ProposalNumber,
            proposal.ProposalStatus.ToString(),
            proposal.SubTotalAmount,
            proposal.TaxAmount,
            proposal.TotalAmount,
            proposal.ProposalRemarks,
            proposal.CustomerRemarks,
            proposal.GeneratedDateUtc,
            proposal.DecisionDateUtc,
            proposal.Lines
                .Where(line => !line.IsDeleted)
                .OrderBy(line => line.SortOrder)
                .ThenBy(line => line.InstallationProposalLineId)
                .Select(line => new InstallationProposalLineResponse(
                    line.InstallationProposalLineId,
                    line.LineDescription,
                    line.Quantity,
                    line.UnitPrice,
                    line.LineTotal,
                    line.Remarks))
                .ToArray());
    }

    public static InstallationChecklistItemResponse MapChecklist(InstallationChecklist checklist)
    {
        var latestResponse = checklist.Responses
            .Where(response => !response.IsDeleted)
            .OrderByDescending(response => response.ResponseDateUtc ?? response.DateCreated)
            .FirstOrDefault();

        return new InstallationChecklistItemResponse(
            checklist.InstallationChecklistId,
            checklist.ChecklistTitle,
            checklist.ChecklistDescription,
            checklist.IsMandatory,
            latestResponse?.IsCompleted ?? false,
            latestResponse?.ResponseRemarks ?? string.Empty,
            latestResponse?.ResponseDateUtc);
    }

    public static InstallationExecutionOrderResponse MapOrder(InstallationOrder order)
    {
        return new InstallationExecutionOrderResponse(
            order.InstallationOrderId,
            order.InstallationOrderNumber,
            order.CurrentStatus.ToString(),
            order.ScheduledInstallationDateUtc,
            order.ExecutionStartedDateUtc,
            order.ExecutionCompletedDateUtc,
            order.TechnicianId,
            order.Technician?.TechnicianName,
            order.HelperCount);
    }

    public static InstallationCommissioningResponse MapCommissioning(CommissioningCertificate certificate)
    {
        return new InstallationCommissioningResponse(
            certificate.CommissioningCertificateId,
            certificate.CertificateNumber,
            certificate.WarrantyRegistrationNumber,
            certificate.CommissioningDateUtc,
            certificate.CustomerConfirmationName,
            certificate.CustomerSignatureName,
            certificate.IsAccepted,
            certificate.Remarks);
    }

    public static InstallationStatusHistoryResponse MapStatusHistory(InstallationStatusHistory history)
    {
        return new InstallationStatusHistoryResponse(
            history.InstallationStatusHistoryId,
            history.PreviousStatus.ToString(),
            history.CurrentStatus.ToString(),
            history.Remarks,
            history.ChangedByRole,
            history.CreatedBy,
            history.ChangedDateUtc);
    }

    public static string BuildChecklistSnapshotJson(InstallationLead installation)
    {
        var snapshot = installation.Checklists
            .Where(checklist => !checklist.IsDeleted)
            .OrderBy(checklist => checklist.SortOrder)
            .ThenBy(checklist => checklist.InstallationChecklistId)
            .Select(checklist =>
            {
                var response = checklist.Responses
                    .Where(item => !item.IsDeleted)
                    .OrderByDescending(item => item.ResponseDateUtc ?? item.DateCreated)
                    .FirstOrDefault();

                return new
                {
                    checklist.ChecklistTitle,
                    checklist.ChecklistDescription,
                    checklist.IsMandatory,
                    IsCompleted = response?.IsCompleted ?? false,
                    ResponseRemarks = response?.ResponseRemarks ?? string.Empty,
                    ResponseDateUtc = response?.ResponseDateUtc
                };
            });

        return JsonSerializer.Serialize(snapshot);
    }
}
