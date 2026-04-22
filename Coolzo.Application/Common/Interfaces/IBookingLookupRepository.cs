using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface IBookingLookupRepository
{
    Task<IReadOnlyCollection<ServiceCategory>> ListServiceCategoriesAsync(string? search, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Service>> ListServicesAsync(long? serviceCategoryId, string? search, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<AcType>> ListAcTypesAsync(string? search, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Tonnage>> ListTonnagesAsync(string? search, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Brand>> ListBrandsAsync(string? search, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Zone>> ListZonesAsync(string? search, CancellationToken cancellationToken);

    Task<Zone?> GetZoneByPincodeAsync(string pincode, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SlotAvailability>> ListAvailableSlotsAsync(long zoneId, DateOnly slotDate, CancellationToken cancellationToken);

    Task<Service?> GetServiceByIdAsync(long serviceId, CancellationToken cancellationToken);

    Task<AcType?> GetAcTypeByIdAsync(long acTypeId, CancellationToken cancellationToken);

    Task<Tonnage?> GetTonnageByIdAsync(long tonnageId, CancellationToken cancellationToken);

    Task<Brand?> GetBrandByIdAsync(long brandId, CancellationToken cancellationToken);

    Task<SlotAvailability?> GetSlotAvailabilityByIdAsync(long slotAvailabilityId, CancellationToken cancellationToken);
}
