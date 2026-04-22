using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class BookingLookupRepository : IBookingLookupRepository
{
    private readonly CoolzoDbContext _dbContext;

    public BookingLookupRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ServiceCategory>> ListServiceCategoriesAsync(string? search, CancellationToken cancellationToken)
    {
        var query = _dbContext.ServiceCategories
            .AsNoTracking()
            .Where(entity => entity.IsActive && !entity.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(entity => entity.CategoryName.Contains(search) || entity.Description.Contains(search));
        }

        return await query
            .OrderBy(entity => entity.SortOrder)
            .ThenBy(entity => entity.CategoryName)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Service>> ListServicesAsync(long? serviceCategoryId, string? search, CancellationToken cancellationToken)
    {
        var query = _dbContext.Services
            .AsNoTracking()
            .Include(entity => entity.PricingModel)
            .Where(entity => entity.IsActive && !entity.IsDeleted);

        if (serviceCategoryId.HasValue)
        {
            query = query.Where(entity => entity.ServiceCategoryId == serviceCategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(entity => entity.ServiceName.Contains(search) || entity.Summary.Contains(search));
        }

        return await query
            .OrderBy(entity => entity.SortOrder)
            .ThenBy(entity => entity.ServiceName)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AcType>> ListAcTypesAsync(string? search, CancellationToken cancellationToken)
    {
        var query = _dbContext.AcTypes
            .AsNoTracking()
            .Where(entity => entity.IsActive && !entity.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(entity => entity.AcTypeName.Contains(search));
        }

        return await query
            .OrderBy(entity => entity.SortOrder)
            .ThenBy(entity => entity.AcTypeName)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Tonnage>> ListTonnagesAsync(string? search, CancellationToken cancellationToken)
    {
        var query = _dbContext.Tonnages
            .AsNoTracking()
            .Where(entity => entity.IsActive && !entity.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(entity => entity.TonnageName.Contains(search));
        }

        return await query
            .OrderBy(entity => entity.SortOrder)
            .ThenBy(entity => entity.TonnageName)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Brand>> ListBrandsAsync(string? search, CancellationToken cancellationToken)
    {
        var query = _dbContext.Brands
            .AsNoTracking()
            .Where(entity => entity.IsActive && !entity.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(entity => entity.BrandName.Contains(search));
        }

        return await query
            .OrderBy(entity => entity.SortOrder)
            .ThenBy(entity => entity.BrandName)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Zone>> ListZonesAsync(string? search, CancellationToken cancellationToken)
    {
        var query = _dbContext.Zones
            .AsNoTracking()
            .Where(entity => entity.IsActive && !entity.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(
                entity => entity.ZoneName.Contains(search) ||
                    entity.ZoneCode.Contains(search) ||
                    entity.CityName.Contains(search));
        }

        return await query
            .OrderBy(entity => entity.ZoneName)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Zone?> GetZoneByPincodeAsync(string pincode, CancellationToken cancellationToken)
    {
        return await _dbContext.ZonePincodes
            .AsNoTracking()
            .Include(entity => entity.Zone)
            .Where(entity => entity.Pincode == pincode && entity.IsActive && !entity.IsDeleted)
            .Select(entity => entity.Zone)
            .FirstOrDefaultAsync(zone => zone != null && zone.IsActive && !zone.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyCollection<SlotAvailability>> ListAvailableSlotsAsync(long zoneId, DateOnly slotDate, CancellationToken cancellationToken)
    {
        return await _dbContext.SlotAvailabilities
            .AsNoTracking()
            .Include(entity => entity.SlotConfiguration)
            .Where(entity =>
                entity.ZoneId == zoneId &&
                entity.SlotDate == slotDate &&
                !entity.IsDeleted &&
                entity.SlotConfiguration != null &&
                entity.SlotConfiguration.IsActive &&
                !entity.SlotConfiguration.IsDeleted)
            .OrderBy(entity => entity.SlotConfiguration!.StartTime)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Service?> GetServiceByIdAsync(long serviceId, CancellationToken cancellationToken)
    {
        return _dbContext.Services
            .FirstOrDefaultAsync(entity => entity.ServiceId == serviceId && entity.IsActive && !entity.IsDeleted, cancellationToken);
    }

    public Task<AcType?> GetAcTypeByIdAsync(long acTypeId, CancellationToken cancellationToken)
    {
        return _dbContext.AcTypes
            .FirstOrDefaultAsync(entity => entity.AcTypeId == acTypeId && entity.IsActive && !entity.IsDeleted, cancellationToken);
    }

    public Task<Tonnage?> GetTonnageByIdAsync(long tonnageId, CancellationToken cancellationToken)
    {
        return _dbContext.Tonnages
            .FirstOrDefaultAsync(entity => entity.TonnageId == tonnageId && entity.IsActive && !entity.IsDeleted, cancellationToken);
    }

    public Task<Brand?> GetBrandByIdAsync(long brandId, CancellationToken cancellationToken)
    {
        return _dbContext.Brands
            .FirstOrDefaultAsync(entity => entity.BrandId == brandId && entity.IsActive && !entity.IsDeleted, cancellationToken);
    }

    public Task<SlotAvailability?> GetSlotAvailabilityByIdAsync(long slotAvailabilityId, CancellationToken cancellationToken)
    {
        return _dbContext.SlotAvailabilities
            .Include(entity => entity.SlotConfiguration)
            .Include(entity => entity.Zone)
            .FirstOrDefaultAsync(entity => entity.SlotAvailabilityId == slotAvailabilityId && !entity.IsDeleted, cancellationToken);
    }
}
