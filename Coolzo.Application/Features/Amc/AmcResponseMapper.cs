using Coolzo.Contracts.Responses.Amc;
using Coolzo.Domain.Entities;

namespace Coolzo.Application.Features.Amc;

internal static class AmcResponseMapper
{
    public static AmcPlanResponse ToAmcPlan(AmcPlan amcPlan)
    {
        return new AmcPlanResponse(
            amcPlan.AmcPlanId,
            amcPlan.PlanName,
            amcPlan.PlanDescription,
            amcPlan.DurationInMonths,
            amcPlan.VisitCount,
            amcPlan.PriceAmount,
            amcPlan.IsActive,
            amcPlan.TermsAndConditions);
    }

    public static CustomerAmcResponse ToCustomerAmc(CustomerAmc customerAmc)
    {
        return new CustomerAmcResponse(
            customerAmc.CustomerAmcId,
            customerAmc.CustomerId,
            customerAmc.Customer?.CustomerName ?? string.Empty,
            customerAmc.AmcPlanId,
            customerAmc.AmcPlan?.PlanName ?? string.Empty,
            customerAmc.JobCardId,
            customerAmc.JobCard?.JobCardNumber ?? string.Empty,
            customerAmc.InvoiceHeaderId,
            customerAmc.InvoiceHeader?.InvoiceNumber ?? string.Empty,
            customerAmc.CurrentStatus.ToString(),
            customerAmc.StartDateUtc,
            customerAmc.EndDateUtc,
            customerAmc.TotalVisitCount,
            customerAmc.ConsumedVisitCount,
            customerAmc.PriceAmount,
            customerAmc.Visits
                .Where(visit => !visit.IsDeleted)
                .OrderBy(visit => visit.VisitNumber)
                .Select(ToVisit)
                .ToArray());
    }

    private static AmcVisitScheduleResponse ToVisit(AmcVisitSchedule amcVisitSchedule)
    {
        return new AmcVisitScheduleResponse(
            amcVisitSchedule.AmcVisitScheduleId,
            amcVisitSchedule.VisitNumber,
            amcVisitSchedule.ScheduledDate,
            amcVisitSchedule.CurrentStatus.ToString(),
            amcVisitSchedule.ServiceRequestId,
            amcVisitSchedule.ServiceRequest?.ServiceRequestNumber,
            amcVisitSchedule.CompletedDateUtc,
            amcVisitSchedule.VisitRemarks);
    }
}
