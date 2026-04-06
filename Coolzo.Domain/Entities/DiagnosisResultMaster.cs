namespace Coolzo.Domain.Entities;

public sealed class DiagnosisResultMaster : AuditableEntity
{
    public long DiagnosisResultMasterId { get; set; }

    public string ResultName { get; set; } = string.Empty;

    public string ResultDescription { get; set; } = string.Empty;

    public ICollection<JobDiagnosis> JobDiagnoses { get; set; } = new List<JobDiagnosis>();
}
