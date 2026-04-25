using Coolzo.Application.Features.GapPhaseE.TechnicianOnboarding;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.GapPhaseE;
using Coolzo.Contracts.Responses.GapPhaseE;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/technicians/{technicianId:long}/skill-assessments")]
public sealed class SkillAssessmentController : ApiControllerBase
{
    private readonly ISender _sender;

    public SkillAssessmentController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Policy = PermissionNames.UserUpdate)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SkillAssessmentDetailResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<SkillAssessmentDetailResponse>>>> CreateAsync(
        [FromRoute] long technicianId,
        [FromBody] CreateSkillAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateSkillAssessmentPhaseECommand(
                technicianId,
                request.SkillTagId,
                request.AssessmentCode,
                request.AssessmentName,
                request.Remarks),
            cancellationToken);

        return Success(response, "Skill assessment created successfully.");
    }

    [Authorize(Policy = PermissionNames.TechnicianRead)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SkillAssessmentDetailResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<SkillAssessmentDetailResponse>>>> GetListAsync(
        [FromRoute] long technicianId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetSkillAssessmentListQuery(technicianId), cancellationToken);
        return Success(response);
    }

    [Authorize(Policy = PermissionNames.UserUpdate)]
    [HttpPost("{assessmentId:long}/submit-result")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SkillAssessmentDetailResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<SkillAssessmentDetailResponse>>>> SubmitResultAsync(
        [FromRoute] long technicianId,
        [FromRoute] long assessmentId,
        [FromBody] SubmitSkillAssessmentResultRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new SubmitSkillAssessmentResultCommand(
                technicianId,
                assessmentId,
                request.ScorePercentage,
                request.PassFlag,
                request.Remarks),
            cancellationToken);

        return Success(response, "Skill assessment result submitted successfully.");
    }
}
