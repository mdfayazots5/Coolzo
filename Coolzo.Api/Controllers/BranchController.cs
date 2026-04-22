using System.Text.Json;
using Asp.Versioning;
using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Branch;
using Coolzo.Contracts.Responses.Branch;
using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Persistence.Context;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/branches")]
public sealed class BranchController : ApiControllerBase
{
    private const string BranchMasterType = "Branch";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IAdminConfigurationRepository _adminConfigurationRepository;
    private readonly CoolzoDbContext _dbContext;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public BranchController(
        IAdminConfigurationRepository adminConfigurationRepository,
        CoolzoDbContext dbContext,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    [HttpGet]
    [Authorize(Policy = PermissionNames.UserRead)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<BranchResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<BranchResponse>>>> GetAsync(
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var records = await _adminConfigurationRepository.SearchDynamicMasterRecordsAsync(
            BranchMasterType,
            searchTerm,
            isActive,
            cancellationToken);
        var response = await Task.WhenAll(records.Select(record => MapAsync(record, cancellationToken)));

        return Success((IReadOnlyCollection<BranchResponse>)response);
    }

    [HttpGet("{branchId:int}")]
    [Authorize(Policy = PermissionNames.UserRead)]
    [ProducesResponseType(typeof(ApiResponse<BranchResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<BranchResponse>>> GetByIdAsync(
        [FromRoute] int branchId,
        CancellationToken cancellationToken)
    {
        var record = await _adminConfigurationRepository.GetDynamicMasterRecordByIdAsync(branchId, cancellationToken);

        if (record is null || !record.MasterType.Equals(BranchMasterType, StringComparison.OrdinalIgnoreCase) || record.IsDeleted)
        {
            return NotFound();
        }

        return Success(await MapAsync(record, cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = PermissionNames.UserCreate)]
    [ProducesResponseType(typeof(ApiResponse<BranchResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<BranchResponse>>> CreateAsync(
        [FromBody] BranchUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var branch = new DynamicMasterRecord
        {
            MasterType = BranchMasterType,
            MasterCode = GenerateMasterCode(request.Name),
            MasterLabel = request.Name.Trim(),
            MasterValue = await BuildPayloadAsync(request, cancellationToken),
            Description = request.Address.Trim(),
            IsActive = request.IsActive,
            IsPublished = true,
            SortOrder = 0,
            CreatedBy = ResolveActorName(),
            DateCreated = _currentDateTime.UtcNow,
            IPAddress = ResolveIpAddress()
        };

        await _adminConfigurationRepository.AddDynamicMasterRecordAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Success(await MapAsync(branch, cancellationToken), "Branch created successfully.");
    }

    [HttpPut("{branchId:int}")]
    [Authorize(Policy = PermissionNames.UserUpdate)]
    [ProducesResponseType(typeof(ApiResponse<BranchResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<BranchResponse>>> UpdateAsync(
        [FromRoute] int branchId,
        [FromBody] BranchUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var branch = await _adminConfigurationRepository.GetDynamicMasterRecordByIdAsync(branchId, cancellationToken);

        if (branch is null || !branch.MasterType.Equals(BranchMasterType, StringComparison.OrdinalIgnoreCase) || branch.IsDeleted)
        {
            return NotFound();
        }

        branch.MasterLabel = request.Name.Trim();
        branch.MasterValue = await BuildPayloadAsync(request, cancellationToken);
        branch.Description = request.Address.Trim();
        branch.IsActive = request.IsActive;
        branch.LastUpdated = _currentDateTime.UtcNow;
        branch.UpdatedBy = ResolveActorName();
        branch.IPAddress = ResolveIpAddress();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Success(await MapAsync(branch, cancellationToken), "Branch updated successfully.");
    }

    private async Task<BranchResponse> MapAsync(DynamicMasterRecord record, CancellationToken cancellationToken)
    {
        var payload = ParsePayload(record.MasterValue);
        var branchId = checked((int)record.DynamicMasterRecordId);
        var managerName = payload.ManagerName;

        if (payload.ManagerId.HasValue)
        {
            managerName = await _dbContext.Users
                .AsNoTracking()
                .Where(user => !user.IsDeleted && user.UserId == payload.ManagerId.Value)
                .Select(user => user.FullName)
                .FirstOrDefaultAsync(cancellationToken) ?? managerName;
        }

        var technicianCount = await _dbContext.Users
            .AsNoTracking()
            .Where(user =>
                !user.IsDeleted &&
                user.IsActive &&
                user.BranchId == branchId &&
                user.UserRoles.Any(userRole =>
                    !userRole.IsDeleted &&
                    userRole.Role != null &&
                    userRole.Role.IsActive &&
                    !userRole.Role.IsDeleted &&
                    (userRole.Role.RoleName == RoleNames.Technician || userRole.Role.RoleName == RoleNames.Helper)))
            .CountAsync(cancellationToken);
        var serviceRequestCount = await _dbContext.ServiceRequests
            .AsNoTracking()
            .CountAsync(serviceRequest =>
                !serviceRequest.IsDeleted &&
                serviceRequest.BranchId == branchId &&
                serviceRequest.CurrentStatus != ServiceRequestStatus.Cancelled,
                cancellationToken);

        return new BranchResponse(
            branchId,
            record.MasterLabel,
            payload.City,
            payload.Address,
            payload.ManagerId,
            managerName,
            payload.Zones,
            record.IsActive,
            technicianCount,
            serviceRequestCount);
    }

    private async Task<string> BuildPayloadAsync(BranchUpsertRequest request, CancellationToken cancellationToken)
    {
        string? managerName = null;

        if (request.ManagerId.HasValue)
        {
            managerName = await _dbContext.Users
                .AsNoTracking()
                .Where(user => !user.IsDeleted && user.UserId == request.ManagerId.Value)
                .Select(user => user.FullName)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return JsonSerializer.Serialize(
            new BranchPayload(
                request.City.Trim(),
                request.Address.Trim(),
                request.ManagerId,
                managerName,
                NormalizeZones(request.Zones)),
            JsonOptions);
    }

    private static BranchPayload ParsePayload(string rawPayload)
    {
        if (string.IsNullOrWhiteSpace(rawPayload))
        {
            return new BranchPayload(string.Empty, string.Empty, null, null, []);
        }

        return JsonSerializer.Deserialize<BranchPayload>(rawPayload, JsonOptions) ??
            new BranchPayload(string.Empty, string.Empty, null, null, []);
    }

    private static string GenerateMasterCode(string branchName)
    {
        var slug = new string(
            branchName
                .Trim()
                .ToLowerInvariant()
                .Select(character => char.IsLetterOrDigit(character) ? character : '-')
                .ToArray())
            .Trim('-');
        var generatedCode = $"{slug}-{Guid.NewGuid():N}";

        return generatedCode[..Math.Min(64, generatedCode.Length)];
    }

    private static IReadOnlyCollection<string> NormalizeZones(IReadOnlyCollection<string>? zones)
    {
        return (zones ?? [])
            .Select(zone => zone.Trim())
            .Where(zone => !string.IsNullOrWhiteSpace(zone))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private string ResolveActorName()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.UserName)
            ? "BranchManagement"
            : _currentUserContext.UserName;
    }

    private string ResolveIpAddress()
    {
        return string.IsNullOrWhiteSpace(_currentUserContext.IPAddress)
            ? "127.0.0.1"
            : _currentUserContext.IPAddress;
    }

    private sealed record BranchPayload(
        string City,
        string Address,
        long? ManagerId,
        string? ManagerName,
        IReadOnlyCollection<string> Zones);
}
