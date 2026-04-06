using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class CustomerAmc : AuditableEntity
{
    public long CustomerAmcId { get; set; }

    public long CustomerId { get; set; }

    public long AmcPlanId { get; set; }

    public long JobCardId { get; set; }

    public long InvoiceHeaderId { get; set; }

    public AmcSubscriptionStatus CurrentStatus { get; set; } = AmcSubscriptionStatus.Active;

    public DateTime StartDateUtc { get; set; }

    public DateTime EndDateUtc { get; set; }

    public int TotalVisitCount { get; set; }

    public int ConsumedVisitCount { get; set; }

    public decimal PriceAmount { get; set; }

    public Customer? Customer { get; set; }

    public AmcPlan? AmcPlan { get; set; }

    public JobCard? JobCard { get; set; }

    public InvoiceHeader? InvoiceHeader { get; set; }

    public ICollection<AmcVisitSchedule> Visits { get; set; } = new List<AmcVisitSchedule>();
}
