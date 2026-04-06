namespace Coolzo.Application.Common.Interfaces;

public interface ICurrentUserContext
{
    long? UserId { get; }

    string UserName { get; }

    bool IsAuthenticated { get; }

    string TraceId { get; }

    string IPAddress { get; }

    IReadOnlyCollection<string> Roles { get; }

    IReadOnlyCollection<string> Permissions { get; }
}
