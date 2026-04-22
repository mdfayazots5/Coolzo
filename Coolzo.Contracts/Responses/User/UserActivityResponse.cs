namespace Coolzo.Contracts.Responses.User;

public sealed record UserActivityResponse
(
    string ActivityId,
    string ActionName,
    string StatusName,
    string ActorName,
    string Description,
    DateTime TimestampUtc
);
