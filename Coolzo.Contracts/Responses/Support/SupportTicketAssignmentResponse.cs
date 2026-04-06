namespace Coolzo.Contracts.Responses.Support;

public sealed record SupportTicketAssignmentResponse
(
    long SupportTicketAssignmentId,
    long AssignedUserId,
    string AssignedUserName,
    string AssignmentRemarks,
    DateTime AssignedDateUtc,
    bool IsActiveAssignment
);
