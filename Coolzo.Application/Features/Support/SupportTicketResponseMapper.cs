using Coolzo.Contracts.Common;
using Coolzo.Contracts.Responses.Support;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;

namespace Coolzo.Application.Features.Support;

internal static class SupportTicketResponseMapper
{
    public static SupportTicketListItemResponse ToListItem(SupportTicket supportTicket)
    {
        var links = supportTicket.Links
            .Where(link => !link.IsDeleted)
            .OrderBy(link => link.SupportTicketLinkId)
            .ToArray();
        var activeAssignment = GetActiveAssignment(supportTicket);

        return new SupportTicketListItemResponse(
            supportTicket.SupportTicketId,
            supportTicket.TicketNumber,
            supportTicket.Subject,
            supportTicket.Customer?.CustomerName ?? string.Empty,
            supportTicket.Customer?.MobileNumber ?? string.Empty,
            links.Length == 1 ? links[0].LinkedEntityType.ToString() : links.FirstOrDefault()?.LinkedEntityType.ToString(),
            BuildLinkedEntitySummary(links),
            supportTicket.Category?.CategoryName ?? string.Empty,
            supportTicket.Priority?.PriorityName ?? string.Empty,
            supportTicket.CurrentStatus.ToString(),
            activeAssignment?.AssignedUserId,
            activeAssignment?.AssignedUser?.FullName,
            supportTicket.DateCreated,
            supportTicket.LastUpdated);
    }

    public static SupportTicketDetailResponse ToDetail(
        SupportTicket supportTicket,
        bool includeInternalReplies,
        bool includeEscalations,
        bool canCustomerClose,
        bool canManage)
    {
        var links = supportTicket.Links
            .Where(link => !link.IsDeleted)
            .OrderBy(link => link.SupportTicketLinkId)
            .Select(ToLinkResponse)
            .ToArray();
        var assignments = supportTicket.Assignments
            .Where(assignment => !assignment.IsDeleted)
            .OrderByDescending(assignment => assignment.AssignedDateUtc)
            .Select(ToAssignmentResponse)
            .ToArray();
        var replies = supportTicket.Replies
            .Where(reply => !reply.IsDeleted && (includeInternalReplies || !reply.IsInternalOnly))
            .OrderBy(reply => reply.ReplyDateUtc)
            .Select(ToReplyResponse)
            .ToArray();
        var escalations = includeEscalations
            ? supportTicket.Escalations
                .Where(escalation => !escalation.IsDeleted)
                .OrderByDescending(escalation => escalation.EscalatedDateUtc)
                .Select(ToEscalationResponse)
                .ToArray()
            : Array.Empty<SupportTicketEscalationResponse>();
        var statusHistory = supportTicket.StatusHistories
            .Where(history => !history.IsDeleted)
            .OrderBy(history => history.StatusDateUtc)
            .Select(ToStatusHistoryResponse)
            .ToArray();
        var activeAssignment = GetActiveAssignment(supportTicket);

        return new SupportTicketDetailResponse(
            supportTicket.SupportTicketId,
            supportTicket.TicketNumber,
            supportTicket.Subject,
            supportTicket.Description,
            supportTicket.CustomerId,
            supportTicket.Customer?.CustomerName ?? string.Empty,
            supportTicket.Customer?.MobileNumber ?? string.Empty,
            supportTicket.Customer?.EmailAddress ?? string.Empty,
            supportTicket.SupportTicketCategoryId,
            supportTicket.Category?.CategoryName ?? string.Empty,
            supportTicket.SupportTicketPriorityId,
            supportTicket.Priority?.PriorityName ?? string.Empty,
            supportTicket.CurrentStatus.ToString(),
            activeAssignment?.AssignedUserId,
            activeAssignment?.AssignedUser?.FullName,
            supportTicket.DateCreated,
            supportTicket.LastUpdated,
            canCustomerClose,
            canManage,
            links,
            assignments,
            replies,
            escalations,
            statusHistory);
    }

    public static IReadOnlyCollection<SupportTicketReplyResponse> ToReplyResponses(
        SupportTicket supportTicket,
        bool includeInternalReplies)
    {
        return supportTicket.Replies
            .Where(reply => !reply.IsDeleted && (includeInternalReplies || !reply.IsInternalOnly))
            .OrderBy(reply => reply.ReplyDateUtc)
            .Select(ToReplyResponse)
            .ToArray();
    }

    public static IReadOnlyCollection<SupportTicketEscalationResponse> ToEscalationResponses(SupportTicket supportTicket)
    {
        return supportTicket.Escalations
            .Where(escalation => !escalation.IsDeleted)
            .OrderByDescending(escalation => escalation.EscalatedDateUtc)
            .Select(ToEscalationResponse)
            .ToArray();
    }

    public static IReadOnlyCollection<SupportTicketStatusHistoryResponse> ToStatusHistoryResponses(SupportTicket supportTicket)
    {
        return supportTicket.StatusHistories
            .Where(history => !history.IsDeleted)
            .OrderBy(history => history.StatusDateUtc)
            .Select(ToStatusHistoryResponse)
            .ToArray();
    }

    public static SupportTicketJobAlertResponse ToJobAlertResponse(Coolzo.Application.Common.Models.SupportJobAlertView alert)
    {
        return new SupportTicketJobAlertResponse(
            alert.HasLinkedTickets,
            alert.TotalLinkedTickets,
            alert.OpenLinkedTickets,
            alert.LatestTicketNumber,
            alert.LatestStatus,
            alert.LatestSubject);
    }

    public static SupportTicketLookupDataResponse ToLookupDataResponse(
        IReadOnlyCollection<SupportTicketCategory> categories,
        IReadOnlyCollection<SupportTicketPriority> priorities)
    {
        return new SupportTicketLookupDataResponse(
            categories
                .OrderBy(category => category.SortOrder)
                .ThenBy(category => category.CategoryName)
                .Select(category => new LookupItemResponse(category.SupportTicketCategoryId, category.CategoryName))
                .ToArray(),
            priorities
                .OrderBy(priority => priority.PriorityRank)
                .ThenBy(priority => priority.SortOrder)
                .ThenBy(priority => priority.PriorityName)
                .Select(priority => new LookupItemResponse(priority.SupportTicketPriorityId, priority.PriorityName))
                .ToArray(),
            Enum.GetValues<SupportTicketStatus>()
                .Select(status => new LookupItemResponse((long)status, status.ToString()))
                .ToArray());
    }

    private static SupportTicketLinkResponse ToLinkResponse(SupportTicketLink supportTicketLink)
    {
        return new SupportTicketLinkResponse(
            supportTicketLink.SupportTicketLinkId,
            supportTicketLink.LinkedEntityType.ToString(),
            supportTicketLink.LinkedEntityId,
            supportTicketLink.LinkReference,
            supportTicketLink.LinkSummary);
    }

    private static SupportTicketAssignmentResponse ToAssignmentResponse(SupportTicketAssignment supportTicketAssignment)
    {
        return new SupportTicketAssignmentResponse(
            supportTicketAssignment.SupportTicketAssignmentId,
            supportTicketAssignment.AssignedUserId,
            supportTicketAssignment.AssignedUser?.FullName ?? string.Empty,
            supportTicketAssignment.AssignmentRemarks,
            supportTicketAssignment.AssignedDateUtc,
            supportTicketAssignment.IsActiveAssignment);
    }

    private static SupportTicketReplyResponse ToReplyResponse(SupportTicketReply supportTicketReply)
    {
        return new SupportTicketReplyResponse(
            supportTicketReply.SupportTicketReplyId,
            supportTicketReply.ReplyText,
            supportTicketReply.IsInternalOnly,
            supportTicketReply.IsFromCustomer,
            supportTicketReply.CreatedBy,
            supportTicketReply.ReplyDateUtc);
    }

    private static SupportTicketEscalationResponse ToEscalationResponse(SupportTicketEscalation supportTicketEscalation)
    {
        return new SupportTicketEscalationResponse(
            supportTicketEscalation.SupportTicketEscalationId,
            supportTicketEscalation.EscalationTarget,
            supportTicketEscalation.EscalationRemarks,
            supportTicketEscalation.CreatedBy,
            supportTicketEscalation.EscalatedDateUtc);
    }

    private static SupportTicketStatusHistoryResponse ToStatusHistoryResponse(SupportTicketStatusHistory supportTicketStatusHistory)
    {
        return new SupportTicketStatusHistoryResponse(
            supportTicketStatusHistory.SupportTicketStatusHistoryId,
            supportTicketStatusHistory.SupportTicketStatus.ToString(),
            supportTicketStatusHistory.Remarks,
            supportTicketStatusHistory.CreatedBy,
            supportTicketStatusHistory.StatusDateUtc);
    }

    private static SupportTicketAssignment? GetActiveAssignment(SupportTicket supportTicket)
    {
        return supportTicket.Assignments
            .Where(assignment => assignment.IsActiveAssignment && !assignment.IsDeleted)
            .OrderByDescending(assignment => assignment.AssignedDateUtc)
            .FirstOrDefault();
    }

    private static string BuildLinkedEntitySummary(IReadOnlyCollection<SupportTicketLink> links)
    {
        return links.Count == 0
            ? "No linked entity"
            : string.Join(" | ", links.Select(link => $"{link.LinkedEntityType}: {link.LinkReference}"));
    }
}
