namespace Coolzo.Contracts.Responses.Amc;

public sealed record AmcPlanResponse(
    long AmcPlanId,
    string PlanName,
    string PlanDescription,
    int DurationInMonths,
    int VisitCount,
    decimal PriceAmount,
    bool IsActive,
    string TermsAndConditions);
