namespace Coolzo.Contracts.Requests.GapPhaseA;

public sealed record CreatePartsReturnRequest(
    long ItemId,
    decimal Quantity,
    string ReasonCode,
    string DefectDescription,
    long? TechnicianId,
    long? JobCardId);

public sealed record ApprovePartsReturnRequest(
    string? Remarks);

public sealed record CreateSupplierClaimRequest(
    long PartsReturnId,
    string SupplierClaimReference,
    string? Remarks);
