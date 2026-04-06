using Coolzo.Contracts.Responses.FieldExecution;
using MediatR;

namespace Coolzo.Application.Features.Diagnosis.Commands.SaveJobDiagnosis;

public sealed record SaveJobDiagnosisCommand(
    long ServiceRequestId,
    long? ComplaintIssueMasterId,
    long? DiagnosisResultMasterId,
    string? DiagnosisRemarks) : IRequest<JobDiagnosisSummaryResponse>;
