using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.RespondFeedback;

public sealed record RespondFeedbackCommand(
    long CustomerReviewId,
    string Response) : IRequest<SupportFeedbackResponse>;
