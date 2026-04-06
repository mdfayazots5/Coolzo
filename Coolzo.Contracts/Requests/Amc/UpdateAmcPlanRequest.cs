namespace Coolzo.Contracts.Requests.Amc;

public sealed record UpdateAmcPlanRequest(
    string PlanName,
    string? PlanDescription,
    int DurationInMonths,
    int VisitCount,
    decimal PriceAmount,
    bool IsActive,
    string? TermsAndConditions);
