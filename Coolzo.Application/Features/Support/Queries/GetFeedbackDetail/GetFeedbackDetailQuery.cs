using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Queries.GetFeedbackDetail;

public sealed record GetFeedbackDetailQuery(long CustomerReviewId) : IRequest<SupportFeedbackResponse>;
