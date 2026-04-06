using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class InstallationLead : AuditableEntity
{
    public long InstallationId { get; set; }

    public string InstallationNumber { get; set; } = string.Empty;

    public long? LeadId { get; set; }

    public long CustomerId { get; set; }

    public long CustomerAddressId { get; set; }

    public long? AssignedTechnicianId { get; set; }

    public int NumberOfUnits { get; set; }

    public string InstallationType { get; set; } = string.Empty;

    public string SiteNotes { get; set; } = string.Empty;

    public DateTime? SurveyDateUtc { get; set; }

    public InstallationApprovalStatus ApprovalStatus { get; set; } = InstallationApprovalStatus.Pending;

    public InstallationLifecycleStatus InstallationStatus { get; set; } = InstallationLifecycleStatus.LeadCreated;

    public DateTime? ProposalApprovedDateUtc { get; set; }

    public DateTime? ScheduledInstallationDateUtc { get; set; }

    public DateTime? InstallationStartedDateUtc { get; set; }

    public DateTime? InstallationCompletedDateUtc { get; set; }

    public DateTime? CommissionedDateUtc { get; set; }

    public Lead? Lead { get; set; }

    public Customer? Customer { get; set; }

    public CustomerAddress? CustomerAddress { get; set; }

    public Technician? AssignedTechnician { get; set; }

    public ICollection<InstallationSurvey> Surveys { get; set; } = new List<InstallationSurvey>();

    public ICollection<InstallationProposal> Proposals { get; set; } = new List<InstallationProposal>();

    public ICollection<InstallationChecklist> Checklists { get; set; } = new List<InstallationChecklist>();

    public ICollection<InstallationStatusHistory> StatusHistories { get; set; } = new List<InstallationStatusHistory>();

    public ICollection<InstallationOrder> Orders { get; set; } = new List<InstallationOrder>();

    public ICollection<CommissioningCertificate> CommissioningCertificates { get; set; } = new List<CommissioningCertificate>();
}

public sealed class InstallationSurvey : AuditableEntity
{
    public long InstallationSurveyId { get; set; }

    public long InstallationId { get; set; }

    public long? TechnicianId { get; set; }

    public DateTime SurveyDateUtc { get; set; }

    public DateTime? CompletedDateUtc { get; set; }

    public string SiteConditionSummary { get; set; } = string.Empty;

    public bool ElectricalReadiness { get; set; }

    public bool AccessReadiness { get; set; }

    public string SafetyRiskNotes { get; set; } = string.Empty;

    public string RecommendedAction { get; set; } = string.Empty;

    public decimal EstimatedMaterialCost { get; set; }

    public string MeasurementsJson { get; set; } = string.Empty;

    public string PhotoUrlsJson { get; set; } = string.Empty;

    public InstallationLead? Installation { get; set; }

    public Technician? Technician { get; set; }

    public ICollection<InstallationSurveyItem> Items { get; set; } = new List<InstallationSurveyItem>();
}

public sealed class InstallationSurveyItem : AuditableEntity
{
    public long InstallationSurveyItemId { get; set; }

    public long InstallationSurveyId { get; set; }

    public string ItemTitle { get; set; } = string.Empty;

    public string ItemValue { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public string Remarks { get; set; } = string.Empty;

    public bool IsMandatory { get; set; }

    public InstallationSurvey? Survey { get; set; }
}

public sealed class InstallationProposal : AuditableEntity
{
    public long InstallationProposalId { get; set; }

    public long InstallationId { get; set; }

    public string ProposalNumber { get; set; } = string.Empty;

    public InstallationProposalStatus ProposalStatus { get; set; } = InstallationProposalStatus.Draft;

    public decimal SubTotalAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string ProposalRemarks { get; set; } = string.Empty;

    public string CustomerRemarks { get; set; } = string.Empty;

    public DateTime GeneratedDateUtc { get; set; }

    public DateTime? DecisionDateUtc { get; set; }

    public InstallationLead? Installation { get; set; }

    public ICollection<InstallationProposalLine> Lines { get; set; } = new List<InstallationProposalLine>();
}

public sealed class InstallationProposalLine : AuditableEntity
{
    public long InstallationProposalLineId { get; set; }

    public long InstallationProposalId { get; set; }

    public string LineDescription { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }

    public string Remarks { get; set; } = string.Empty;

    public InstallationProposal? Proposal { get; set; }
}

public sealed class InstallationChecklist : AuditableEntity
{
    public long InstallationChecklistId { get; set; }

    public long InstallationId { get; set; }

    public string ChecklistTitle { get; set; } = string.Empty;

    public string ChecklistDescription { get; set; } = string.Empty;

    public bool IsMandatory { get; set; }

    public InstallationLead? Installation { get; set; }

    public ICollection<InstallationChecklistResponse> Responses { get; set; } = new List<InstallationChecklistResponse>();
}

public sealed class InstallationChecklistResponse : AuditableEntity
{
    public long InstallationChecklistResponseId { get; set; }

    public long InstallationChecklistId { get; set; }

    public long InstallationId { get; set; }

    public bool IsCompleted { get; set; }

    public string ResponseRemarks { get; set; } = string.Empty;

    public DateTime? ResponseDateUtc { get; set; }

    public InstallationChecklist? Checklist { get; set; }

    public InstallationLead? Installation { get; set; }
}

public sealed class InstallationStatusHistory : AuditableEntity
{
    public long InstallationStatusHistoryId { get; set; }

    public long InstallationId { get; set; }

    public InstallationLifecycleStatus PreviousStatus { get; set; } = InstallationLifecycleStatus.LeadCreated;

    public InstallationLifecycleStatus CurrentStatus { get; set; } = InstallationLifecycleStatus.LeadCreated;

    public string Remarks { get; set; } = string.Empty;

    public string ChangedByRole { get; set; } = string.Empty;

    public DateTime ChangedDateUtc { get; set; }

    public InstallationLead? Installation { get; set; }
}
