using Coolzo.Contracts.Responses.Warranty;
using MediatR;

namespace Coolzo.Application.Features.Warranty.Commands.CreateWarrantyClaim;

public sealed record CreateWarrantyClaimCommand(
    long InvoiceId,
    string? ClaimRemarks) : IRequest<WarrantyClaimResponse>;
