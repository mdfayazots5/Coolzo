using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Queries.GetFeedbackList;

public sealed record GetFeedbackListQuery(long? ServiceId) : IRequest<IReadOnlyCollection<SupportFeedbackResponse>>;
