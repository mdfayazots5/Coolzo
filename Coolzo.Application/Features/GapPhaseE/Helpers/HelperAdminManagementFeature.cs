using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.GapPhaseE;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.GapPhaseE.Helpers;

public sealed record UpdateHelperProfileCommand(
    long HelperProfileId,
    string HelperCode,
    string HelperName,
    string MobileNo,
    bool ActiveFlag) : IRequest<HelperDetailResponse>;

public sealed class UpdateHelperProfileCommandValidator : AbstractValidator<UpdateHelperProfileCommand>
{
    public UpdateHelperProfileCommandValidator()
    {
        RuleFor(request => request.HelperProfileId).GreaterThan(0);
        RuleFor(request => request.HelperCode).NotEmpty().MaximumLength(32);
        RuleFor(request => request.HelperName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.MobileNo).Matches("^[0-9]{8,16}$");
    }
}

public sealed class UpdateHelperProfileCommandHandler : IRequestHandler<UpdateHelperProfileCommand, HelperDetailResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGapPhaseERepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateHelperProfileCommandHandler(
        IGapPhaseERepository repository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _repository = repository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<HelperDetailResponse> Handle(UpdateHelperProfileCommand request, CancellationToken cancellationToken)
    {
        var helperProfile = await _repository.GetHelperProfileAsync(request.HelperProfileId, asNoTracking: false, cancellationToken: cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The helper profile could not be found.", 404);

        var normalizedHelperCode = request.HelperCode.Trim();
        if (await _repository.HelperCodeExistsAsync(normalizedHelperCode, request.HelperProfileId, cancellationToken))
        {
            throw new AppException(ErrorCodes.DuplicateValue, "A helper profile already exists for this helper code.", 409);
        }

        var now = _currentDateTime.UtcNow;
        helperProfile.HelperCode = normalizedHelperCode;
        helperProfile.HelperName = request.HelperName.Trim();
        helperProfile.MobileNo = request.MobileNo.Trim();
        helperProfile.ActiveFlag = request.ActiveFlag;
        helperProfile.LastUpdated = now;
        helperProfile.UpdatedBy = HelperWorkflowSupport.ResolveActor(_currentUserContext, "HelperManagement");
        helperProfile.IPAddress = _currentUserContext.IPAddress;

        await _auditLogRepository.AddAsync(
            HelperWorkflowSupport.CreateAuditLog(
                _currentUserContext,
                now,
                "UpdateHelperProfile",
                nameof(HelperProfile),
                helperProfile.HelperProfileId.ToString(),
                helperProfile.HelperName),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await HelperWorkflowSupport.BuildHelperDetailAsync(helperProfile, _repository, cancellationToken);
    }
}
