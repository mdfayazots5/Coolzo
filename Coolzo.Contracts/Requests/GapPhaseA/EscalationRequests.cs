namespace Coolzo.Contracts.Requests.GapPhaseA;

public sealed record CreateEscalationRequest(
    string AlertType,
    string RelatedEntityName,
    string RelatedEntityId,
    string Severity,
    int EscalationLevel,
    int SlaMinutes,
    string? NotificationChain,
    string Message);

public sealed record HandleNoShowRequest(
    string Reason,
    long? PreferredTechnicianId);
