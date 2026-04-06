using System.Security.Claims;
using Coolzo.Application.Common.Interfaces;
using Coolzo.Shared.Constants;
using Microsoft.AspNetCore.Http;

namespace Coolzo.Infrastructure.Identity;

public sealed class HttpCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? UserId
    {
        get
        {
            var rawValue = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            return long.TryParse(rawValue, out var userId) ? userId : null;
        }
    }

    public string UserName => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name) ?? "System";

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public string TraceId => _httpContextAccessor.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");

    public string IPAddress => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

    public IReadOnlyCollection<string> Roles => _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray()
        ?? Array.Empty<string>();

    public IReadOnlyCollection<string> Permissions => _httpContextAccessor.HttpContext?.User.FindAll(CustomClaimTypes.Permission).Select(claim => claim.Value).ToArray()
        ?? Array.Empty<string>();
}
