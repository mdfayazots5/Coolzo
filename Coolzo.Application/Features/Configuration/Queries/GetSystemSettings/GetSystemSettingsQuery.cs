using Coolzo.Contracts.Responses.Configuration;
using MediatR;

namespace Coolzo.Application.Features.Configuration.Queries.GetSystemSettings;

public sealed record GetSystemSettingsQuery : IRequest<IReadOnlyCollection<SystemSettingResponse>>;
