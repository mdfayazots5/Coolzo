using Coolzo.Contracts.Responses.Revisit;
using MediatR;

namespace Coolzo.Application.Features.Revisit.Commands.CreateRevisitRequest;

public sealed record CreateRevisitRequestCommand(
    long OriginalJobCardId,
    string RevisitType,
    DateTime? PreferredVisitDateUtc,
    string IssueSummary,
    string? RequestRemarks,
    long? CustomerAmcId,
    long? WarrantyClaimId,
    decimal? ChargeAmount) : IRequest<RevisitRequestResponse>;
