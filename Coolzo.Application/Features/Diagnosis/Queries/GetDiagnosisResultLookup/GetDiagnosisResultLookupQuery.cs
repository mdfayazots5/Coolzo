using Coolzo.Contracts.Responses.FieldExecution;
using MediatR;

namespace Coolzo.Application.Features.Diagnosis.Queries.GetDiagnosisResultLookup;

public sealed record GetDiagnosisResultLookupQuery(
    string? SearchTerm) : IRequest<IReadOnlyCollection<DiagnosisLookupItemResponse>>;
