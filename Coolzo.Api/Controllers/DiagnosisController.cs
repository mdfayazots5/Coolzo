using Coolzo.Application.Features.Diagnosis.Commands.SaveJobDiagnosis;
using Coolzo.Application.Features.Diagnosis.Queries.GetDiagnosisIssueLookup;
using Coolzo.Application.Features.Diagnosis.Queries.GetDiagnosisResultLookup;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.FieldExecution;
using Coolzo.Contracts.Responses.FieldExecution;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize(Roles = RoleNames.Technician)]
[Route("api")]
public sealed class DiagnosisController : ApiControllerBase
{
    private readonly ISender _sender;

    public DiagnosisController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("diagnosis/lookups/issues")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<DiagnosisLookupItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<DiagnosisLookupItemResponse>>>> GetIssueLookupsAsync(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetDiagnosisIssueLookupQuery(search), cancellationToken);

        return Success(response);
    }

    [HttpGet("diagnosis/lookups/results")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<DiagnosisLookupItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<DiagnosisLookupItemResponse>>>> GetResultLookupsAsync(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetDiagnosisResultLookupQuery(search), cancellationToken);

        return Success(response);
    }

    [HttpPost("technician-jobs/{id:long}/diagnosis")]
    [ProducesResponseType(typeof(ApiResponse<JobDiagnosisSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<JobDiagnosisSummaryResponse>>> SaveDiagnosisAsync(
        [FromRoute] long id,
        [FromBody] SaveJobDiagnosisRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new SaveJobDiagnosisCommand(id, request.ComplaintIssueMasterId, request.DiagnosisResultMasterId, request.DiagnosisRemarks),
            cancellationToken);

        return Success(response, "Job diagnosis saved successfully.");
    }
}
