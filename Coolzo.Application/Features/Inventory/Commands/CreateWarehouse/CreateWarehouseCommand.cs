using Coolzo.Contracts.Responses.Inventory;
using MediatR;

namespace Coolzo.Application.Features.Inventory.Commands.CreateWarehouse;

public sealed record CreateWarehouseCommand(
    string WarehouseCode,
    string WarehouseName,
    string? ContactPerson,
    string? MobileNumber,
    string? EmailAddress,
    string? AddressLine1,
    string? AddressLine2,
    string? Landmark,
    string? CityName,
    string? Pincode,
    bool IsActive) : IRequest<WarehouseResponse>;
