using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface IGapPhaseERepository
{
    Task<bool> TechnicianMobileExistsAsync(string mobileNumber, long? excludedTechnicianId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Technician>> SearchTechniciansAsync(string? searchTerm, int? branchId, CancellationToken cancellationToken);

    Task<Technician?> GetTechnicianAsync(long technicianId, bool asNoTracking, CancellationToken cancellationToken);

    Task AddTechnicianDocumentAsync(TechnicianDocument technicianDocument, CancellationToken cancellationToken);

    Task<TechnicianDocument?> GetTechnicianDocumentAsync(long technicianId, long technicianDocumentId, bool asNoTracking, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TechnicianDocument>> GetTechnicianDocumentsAsync(long technicianId, CancellationToken cancellationToken);

    Task AddSkillAssessmentAsync(SkillAssessment skillAssessment, CancellationToken cancellationToken);

    Task<SkillAssessment?> GetSkillAssessmentAsync(long technicianId, long skillAssessmentId, bool asNoTracking, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SkillAssessment>> GetSkillAssessmentsAsync(long technicianId, CancellationToken cancellationToken);

    Task AddTrainingRecordAsync(TrainingRecord trainingRecord, CancellationToken cancellationToken);

    Task<TrainingRecord?> GetTrainingRecordAsync(long technicianId, long trainingRecordId, bool asNoTracking, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TrainingRecord>> GetTrainingRecordsAsync(long technicianId, CancellationToken cancellationToken);

    Task AddTechnicianActivationLogAsync(TechnicianActivationLog technicianActivationLog, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TechnicianActivationLog>> GetTechnicianActivationLogsAsync(long technicianId, CancellationToken cancellationToken);

    Task<bool> HelperCodeExistsAsync(string helperCode, long? excludedHelperProfileId, CancellationToken cancellationToken);

    Task AddHelperProfileAsync(HelperProfile helperProfile, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<HelperProfile>> SearchHelpersAsync(string? searchTerm, int? branchId, CancellationToken cancellationToken);

    Task<HelperProfile?> GetHelperProfileAsync(long helperProfileId, bool asNoTracking, CancellationToken cancellationToken);

    Task<HelperProfile?> GetHelperProfileByUserIdAsync(long userId, CancellationToken cancellationToken);

    Task AddHelperAssignmentAsync(HelperAssignment helperAssignment, CancellationToken cancellationToken);

    Task<HelperAssignment?> GetActiveHelperAssignmentAsync(long helperProfileId, bool asNoTracking, CancellationToken cancellationToken);

    Task<HelperAssignment?> GetHelperAssignmentByIdAsync(long helperProfileId, long helperAssignmentId, bool asNoTracking, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<HelperAssignment>> GetHelperAssignmentsAsync(long helperProfileId, CancellationToken cancellationToken);

    Task AddHelperAttendanceAsync(HelperAttendance helperAttendance, CancellationToken cancellationToken);

    Task<HelperAttendance?> GetOpenHelperAttendanceAsync(long helperProfileId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<HelperAttendance>> GetHelperAttendancesAsync(long helperProfileId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<HelperTaskChecklist>> GetHelperTaskChecklistsAsync(long? serviceTypeId, CancellationToken cancellationToken);

    Task AddHelperTaskResponseAsync(HelperTaskResponse helperTaskResponse, CancellationToken cancellationToken);

    Task<HelperTaskResponse?> GetHelperTaskResponseAsync(long helperAssignmentId, long helperTaskChecklistId, bool asNoTracking, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<HelperTaskResponse>> GetHelperTaskResponsesAsync(long helperAssignmentId, CancellationToken cancellationToken);

    Task<ServiceRequest?> GetServiceRequestAsync(long serviceRequestId, bool asNoTracking, CancellationToken cancellationToken);

    Task<JobCard?> GetJobCardAsync(long jobCardId, bool asNoTracking, CancellationToken cancellationToken);
}
