using Coolzo.Contracts.Responses.Amc;
using MediatR;

namespace Coolzo.Application.Features.Amc.Commands.GenerateAmcVisits;

public sealed record GenerateAmcVisitsCommand(long CustomerAmcId) : IRequest<CustomerAmcResponse>;
