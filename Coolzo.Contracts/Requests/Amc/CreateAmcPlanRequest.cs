namespace Coolzo.Contracts.Requests.Amc;

public sealed record CreateAmcPlanRequest(
    string PlanName,
    string? PlanDescription,
    int DurationInMonths,
    int VisitCount,
    decimal PriceAmount,
    bool IsActive,
    string? TermsAndConditions);
