using Coolzo.Contracts.Responses.Support;
using MediatR;

namespace Coolzo.Application.Features.Support.Commands.FlagFeedback;

public sealed record FlagFeedbackCommand(
    long CustomerReviewId,
    string Reason) : IRequest<SupportFeedbackResponse>;
