namespace Coolzo.Domain.Entities;

public sealed class AmcPlan : AuditableEntity
{
    public long AmcPlanId { get; set; }

    public string PlanName { get; set; } = string.Empty;

    public string PlanDescription { get; set; } = string.Empty;

    public int DurationInMonths { get; set; }

    public int VisitCount { get; set; }

    public decimal PriceAmount { get; set; }

    public bool IsActive { get; set; } = true;

    public string TermsAndConditions { get; set; } = string.Empty;

    public ICollection<CustomerAmc> CustomerAmcs { get; set; } = new List<CustomerAmc>();
}
