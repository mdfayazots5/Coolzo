using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.PublishFeedback;

public sealed record PublishFeedbackCommand(
    long CustomerReviewId,
    bool Publish) : IRequest<SupportFeedbackResponse>;
