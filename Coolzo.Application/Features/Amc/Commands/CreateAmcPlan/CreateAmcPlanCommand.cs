using Coolzo.Contracts.Responses.Amc;
using MediatR;

namespace Coolzo.Application.Features.Amc.Commands.CreateAmcPlan;

public sealed record CreateAmcPlanCommand(
    string PlanName,
    string? PlanDescription,
    int DurationInMonths,
    int VisitCount,
    decimal PriceAmount,
    bool IsActive,
    string? TermsAndConditions) : IRequest<AmcPlanResponse>;
