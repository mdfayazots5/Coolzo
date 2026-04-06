using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.FieldExecution;
using MediatR;

namespace Coolzo.Application.Features.Diagnosis.Queries.GetDiagnosisIssueLookup;

public sealed class GetDiagnosisIssueLookupQueryHandler : IRequestHandler<GetDiagnosisIssueLookupQuery, IReadOnlyCollection<DiagnosisLookupItemResponse>>
{
    private readonly IFieldLookupRepository _fieldLookupRepository;

    public GetDiagnosisIssueLookupQueryHandler(IFieldLookupRepository fieldLookupRepository)
    {
        _fieldLookupRepository = fieldLookupRepository;
    }

    public async Task<IReadOnlyCollection<DiagnosisLookupItemResponse>> Handle(GetDiagnosisIssueLookupQuery request, CancellationToken cancellationToken)
    {
        var issues = await _fieldLookupRepository.SearchComplaintIssuesAsync(request.SearchTerm, cancellationToken);

        return issues
            .Select(issue => new DiagnosisLookupItemResponse(
                issue.ComplaintIssueMasterId,
                issue.IssueName,
                issue.IssueDescription))
            .ToArray();
    }
}
