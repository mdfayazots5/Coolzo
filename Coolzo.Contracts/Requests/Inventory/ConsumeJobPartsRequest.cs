namespace Coolzo.Contracts.Requests.Inventory;

public sealed record ConsumeJobPartsRequest(
    IReadOnlyCollection<ConsumeJobPartItemRequest> Items);
