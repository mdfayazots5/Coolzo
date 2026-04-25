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
[Route("api/technicians/{technicianId:long}/training-records")]
public sealed class TrainingController : ApiControllerBase
{
    private readonly ISender _sender;

    public TrainingController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Policy = PermissionNames.UserUpdate)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TrainingRecordDetailResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TrainingRecordDetailResponse>>>> CreateAsync(
        [FromRoute] long technicianId,
        [FromBody] CreateTrainingRecordRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateTrainingRecordPhaseECommand(
                technicianId,
                request.TrainingTitle,
                request.TrainingType,
                request.Remarks),
            cancellationToken);

        return Success(response, "Training record created successfully.");
    }

    [Authorize(Policy = PermissionNames.TechnicianRead)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TrainingRecordDetailResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TrainingRecordDetailResponse>>>> GetListAsync(
        [FromRoute] long technicianId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetTrainingRecordListQuery(technicianId), cancellationToken);
        return Success(response);
    }

    [Authorize(Policy = PermissionNames.UserUpdate)]
    [HttpPost("{trainingRecordId:long}/complete")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TrainingRecordDetailResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TrainingRecordDetailResponse>>>> CompleteAsync(
        [FromRoute] long technicianId,
        [FromRoute] long trainingRecordId,
        [FromBody] CompleteTrainingRecordRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CompleteTrainingRecordCommand(
                technicianId,
                trainingRecordId,
                request.CertificationNumber,
                request.ScorePercentage,
                request.CertificateUrl,
                request.Remarks),
            cancellationToken);

        return Success(response, "Training record completed successfully.");
    }
}
