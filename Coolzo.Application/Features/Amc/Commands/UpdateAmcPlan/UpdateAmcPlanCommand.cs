using Coolzo.Contracts.Responses.Amc;
using MediatR;

namespace Coolzo.Application.Features.Amc.Commands.UpdateAmcPlan;

public sealed record UpdateAmcPlanCommand(
    long AmcPlanId,
    string PlanName,
    string? PlanDescription,
    int DurationInMonths,
    int VisitCount,
    decimal PriceAmount,
    bool IsActive,
    string? TermsAndConditions) : IRequest<AmcPlanResponse>;
