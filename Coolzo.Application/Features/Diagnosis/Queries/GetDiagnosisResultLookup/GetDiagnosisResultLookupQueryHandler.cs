using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.FieldExecution;
using MediatR;

namespace Coolzo.Application.Features.Diagnosis.Queries.GetDiagnosisResultLookup;

public sealed class GetDiagnosisResultLookupQueryHandler : IRequestHandler<GetDiagnosisResultLookupQuery, IReadOnlyCollection<DiagnosisLookupItemResponse>>
{
    private readonly IFieldLookupRepository _fieldLookupRepository;

    public GetDiagnosisResultLookupQueryHandler(IFieldLookupRepository fieldLookupRepository)
    {
        _fieldLookupRepository = fieldLookupRepository;
    }

    public async Task<IReadOnlyCollection<DiagnosisLookupItemResponse>> Handle(GetDiagnosisResultLookupQuery request, CancellationToken cancellationToken)
    {
        var results = await _fieldLookupRepository.SearchDiagnosisResultsAsync(request.SearchTerm, cancellationToken);

        return results
            .Select(result => new DiagnosisLookupItemResponse(
                result.DiagnosisResultMasterId,
                result.ResultName,
                result.ResultDescription))
            .ToArray();
    }
}
