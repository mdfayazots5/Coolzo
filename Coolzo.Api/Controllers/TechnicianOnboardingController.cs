using Coolzo.Application.Features.GapPhaseE.TechnicianOnboarding;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.GapPhaseA;
using Coolzo.Contracts.Responses.GapPhaseE;
using Coolzo.Contracts.Responses.GapPhaseA;
using Coolzo.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[Authorize]
[Route("api/technician-onboarding")]
public sealed class TechnicianOnboardingController : ApiControllerBase
{
    private readonly ISender _sender;

    public TechnicianOnboardingController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Policy = PermissionNames.UserCreate)]
    [HttpPost("draft")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianOnboardingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianOnboardingResponse>>> CreateDraftAsync(
        [FromBody] CreateTechnicianDraftRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateTechnicianDraftPhaseECommand(
                request.TechnicianName,
                request.MobileNumber,
                request.EmailAddress,
                request.BaseZoneId,
                request.MaxDailyAssignments),
            cancellationToken);

        return Success(MapLegacyResponse(response), "Technician draft created successfully.");
    }

    [Authorize(Policy = PermissionNames.TechnicianRead)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TechnicianOnboardingListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TechnicianOnboardingListItemResponse>>>> GetListAsync(
        [FromQuery] string? searchTerm,
        [FromQuery] string? status,
        [FromQuery] int? branchId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetTechnicianOnboardingListQuery(searchTerm, status, branchId), cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.TechnicianRead)]
    [HttpGet("{technicianId:long}")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianOnboardingDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianOnboardingDetailResponse>>> GetDetailAsync(
        [FromRoute] long technicianId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetTechnicianOnboardingDetailQuery(technicianId), cancellationToken);

        return Success(response);
    }

    [Authorize(Policy = PermissionNames.UserUpdate)]
    [HttpPost("{technicianId:long}/documents")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianOnboardingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianOnboardingResponse>>> UploadDocumentsAsync(
        [FromRoute] long technicianId,
        [FromBody] UploadTechnicianDocumentsRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new UploadTechnicianDocumentsPhaseECommand(technicianId, request.Documents), cancellationToken);

        return Success(MapLegacyResponse(response), "Technician documents uploaded successfully.");
    }

    [Authorize(Policy = PermissionNames.UserUpdate)]
    [HttpPost("{technicianId:long}/activate")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianOnboardingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianOnboardingResponse>>> ActivateAsync(
        [FromRoute] long technicianId,
        [FromBody] ActivateTechnicianRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new ActivateTechnicianPhaseECommand(
                technicianId,
                request.Remarks?.Trim() ?? "Legacy onboarding activation request.",
                new LegacyActivationBootstrap(
                    request.AssessmentCode,
                    request.ScorePercentage,
                    request.TrainingName,
                    request.CertificationNumber,
                    request.TrainingScorePercentage,
                    request.Remarks)),
            cancellationToken);

        return Success(MapLegacyResponse(response), "Technician activated successfully.");
    }

    private static TechnicianOnboardingResponse MapLegacyResponse(TechnicianOnboardingDetailResponse detail)
    {
        return new TechnicianOnboardingResponse(
            detail.TechnicianId,
            detail.TechnicianCode,
            detail.TechnicianName,
            detail.IsActive,
            detail.Documents.Count,
            detail.SkillAssessments.OrderByDescending(item => item.AssessedOnUtc ?? DateTime.MinValue).FirstOrDefault()?.AssessmentResult ?? "Pending",
            detail.TrainingRecords.Count(item => item.IsCompleted));
    }
}
