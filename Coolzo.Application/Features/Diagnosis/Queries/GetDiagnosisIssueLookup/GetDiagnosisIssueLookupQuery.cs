using Coolzo.Contracts.Responses.FieldExecution;
using MediatR;

namespace Coolzo.Application.Features.Diagnosis.Queries.GetDiagnosisIssueLookup;

public sealed record GetDiagnosisIssueLookupQuery(
    string? SearchTerm) : IRequest<IReadOnlyCollection<DiagnosisLookupItemResponse>>;
