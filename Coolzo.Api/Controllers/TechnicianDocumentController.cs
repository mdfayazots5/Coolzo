using Coolzo.Application.Features.GapPhaseE.TechnicianOnboarding;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.GapPhaseA;
using Coolzo.Contracts.Requests.GapPhaseE;
using Coolzo.Contracts.Responses.GapPhaseE;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/technicians/{technicianId:long}/documents")]
public sealed class TechnicianDocumentController : ApiControllerBase
{
    private readonly ISender _sender;

    public TechnicianDocumentController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Policy = PermissionNames.UserUpdate)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TechnicianOnboardingDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianOnboardingDetailResponse>>> UploadAsync(
        [FromRoute] long technicianId,
        [FromBody] UploadTechnicianDocumentsRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new UploadTechnicianDocumentsPhaseECommand(technicianId, request.Documents), cancellationToken);
        return Success(response, "Technician documents uploaded successfully.");
    }

    [Authorize(Policy = PermissionNames.TechnicianRead)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TechnicianDocumentDetailResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TechnicianDocumentDetailResponse>>>> GetListAsync(
        [FromRoute] long technicianId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetTechnicianDocumentListQuery(technicianId), cancellationToken);
        return Success(response);
    }

    [Authorize(Policy = PermissionNames.UserUpdate)]
    [HttpPost("{documentId:long}/verify")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TechnicianDocumentDetailResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TechnicianDocumentDetailResponse>>>> VerifyAsync(
        [FromRoute] long technicianId,
        [FromRoute] long documentId,
        [FromBody] VerifyTechnicianDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new VerifyTechnicianDocumentCommand(technicianId, documentId, request.Remarks), cancellationToken);
        return Success(response, "Technician document verified successfully.");
    }

    [Authorize(Policy = PermissionNames.UserUpdate)]
    [HttpPost("{documentId:long}/reject")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TechnicianDocumentDetailResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TechnicianDocumentDetailResponse>>>> RejectAsync(
        [FromRoute] long technicianId,
        [FromRoute] long documentId,
        [FromBody] RejectTechnicianDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new RejectTechnicianDocumentCommand(technicianId, documentId, request.Remarks), cancellationToken);
        return Success(response, "Technician document rejected successfully.");
    }
}
