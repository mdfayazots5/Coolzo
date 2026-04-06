namespace Coolzo.Domain.Entities;

public sealed record TechnicianPerformanceReadModel(
    long TotalTechnicians,
    long ActiveTechnicians,
    long TotalAssignedJobs,
    long TotalCompletedJobs,
    decimal AverageCompletionHours,
    IReadOnlyCollection<TechnicianPerformanceItemReadModel> Technicians);

