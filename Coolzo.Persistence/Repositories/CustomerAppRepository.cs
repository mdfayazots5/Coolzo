using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class CustomerAppRepository : ICustomerAppRepository
{
    private readonly CoolzoDbContext _dbContext;

    public CustomerAppRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<CustomerAddress>> ListAddressesAsync(long customerId, CancellationToken cancellationToken)
    {
        return await _dbContext.CustomerAddresses
            .AsNoTracking()
            .Where(entity => entity.CustomerId == customerId && entity.IsActive && !entity.IsDeleted)
            .OrderByDescending(entity => entity.IsDefault)
            .ThenBy(entity => entity.AddressLabel)
            .ToArrayAsync(cancellationToken);
    }

    public Task<CustomerAddress?> GetAddressForUpdateAsync(long customerId, long customerAddressId, CancellationToken cancellationToken)
    {
        return _dbContext.CustomerAddresses
            .FirstOrDefaultAsync(
                entity => entity.CustomerId == customerId &&
                    entity.CustomerAddressId == customerAddressId &&
                    !entity.IsDeleted,
                cancellationToken);
    }

    public Task AddAddressAsync(CustomerAddress customerAddress, CancellationToken cancellationToken)
    {
        return _dbContext.CustomerAddresses.AddAsync(customerAddress, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<CustomerAddress>> ListDefaultAddressesForUpdateAsync(long customerId, long? excludedAddressId, CancellationToken cancellationToken)
    {
        return await _dbContext.CustomerAddresses
            .Where(entity => entity.CustomerId == customerId &&
                entity.IsDefault &&
                !entity.IsDeleted &&
                (!excludedAddressId.HasValue || entity.CustomerAddressId != excludedAddressId.Value))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<CustomerEquipment>> ListEquipmentAsync(long customerId, CancellationToken cancellationToken)
    {
        return await _dbContext.CustomerEquipments
            .AsNoTracking()
            .Where(entity => entity.CustomerId == customerId && entity.IsActive && !entity.IsDeleted)
            .OrderBy(entity => entity.LocationLabel)
            .ThenBy(entity => entity.EquipmentName)
            .ToArrayAsync(cancellationToken);
    }

    public Task<CustomerEquipment?> GetEquipmentForUpdateAsync(long customerId, long customerEquipmentId, CancellationToken cancellationToken)
    {
        return _dbContext.CustomerEquipments
            .FirstOrDefaultAsync(
                entity => entity.CustomerId == customerId &&
                    entity.CustomerEquipmentId == customerEquipmentId &&
                    !entity.IsDeleted,
                cancellationToken);
    }

    public Task AddEquipmentAsync(CustomerEquipment customerEquipment, CancellationToken cancellationToken)
    {
        return _dbContext.CustomerEquipments.AddAsync(customerEquipment, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<CustomerNotification>> ListNotificationsAsync(long customerId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var skip = (pageNumber - 1) * pageSize;

        return await _dbContext.CustomerNotifications
            .AsNoTracking()
            .Where(entity => entity.CustomerId == customerId && !entity.IsDeleted)
            .OrderByDescending(entity => entity.DateCreated)
            .Skip(skip)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountNotificationsAsync(long customerId, CancellationToken cancellationToken)
    {
        return _dbContext.CustomerNotifications.CountAsync(entity => entity.CustomerId == customerId && !entity.IsDeleted, cancellationToken);
    }

    public Task<CustomerNotification?> GetNotificationForUpdateAsync(long customerId, long customerNotificationId, CancellationToken cancellationToken)
    {
        return _dbContext.CustomerNotifications
            .FirstOrDefaultAsync(
                entity => entity.CustomerId == customerId &&
                    entity.CustomerNotificationId == customerNotificationId &&
                    !entity.IsDeleted,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<PromotionalOffer>> ListActiveOffersAsync(DateOnly currentDate, CancellationToken cancellationToken)
    {
        return await _dbContext.PromotionalOffers
            .AsNoTracking()
            .Where(entity => entity.IsActive &&
                !entity.IsDeleted &&
                (!entity.ExpiryDate.HasValue || entity.ExpiryDate.Value >= currentDate))
            .OrderBy(entity => entity.SortOrder)
            .ThenBy(entity => entity.Title)
            .ToArrayAsync(cancellationToken);
    }

    public Task<PromotionalOffer?> GetActiveOfferByCodeAsync(string offerCode, DateOnly currentDate, CancellationToken cancellationToken)
    {
        return _dbContext.PromotionalOffers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                entity => entity.OfferCode == offerCode &&
                    entity.IsActive &&
                    !entity.IsDeleted &&
                    (!entity.ExpiryDate.HasValue || entity.ExpiryDate.Value >= currentDate),
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<CustomerReferral>> ListReferralsAsync(long customerId, CancellationToken cancellationToken)
    {
        return await _dbContext.CustomerReferrals
            .AsNoTracking()
            .Where(entity => entity.CustomerId == customerId && !entity.IsDeleted)
            .OrderByDescending(entity => entity.ReferralDate)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<CustomerLoyaltyTransaction>> ListLoyaltyTransactionsAsync(long customerId, CancellationToken cancellationToken)
    {
        return await _dbContext.CustomerLoyaltyTransactions
            .AsNoTracking()
            .Where(entity => entity.CustomerId == customerId && !entity.IsDeleted)
            .OrderByDescending(entity => entity.DateCreated)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<CustomerReview>> ListReviewsAsync(long? serviceId, CancellationToken cancellationToken)
    {
        var query = _dbContext.CustomerReviews
            .AsNoTracking()
            .Where(entity => entity.IsActive && !entity.IsDeleted);

        if (serviceId.HasValue)
        {
            query = query.Where(entity => entity.ServiceId == serviceId.Value);
        }

        return await query
            .OrderByDescending(entity => entity.DateCreated)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddReviewAsync(CustomerReview customerReview, CancellationToken cancellationToken)
    {
        return _dbContext.CustomerReviews.AddAsync(customerReview, cancellationToken).AsTask();
    }

    public Task AddAppFeedbackAsync(CustomerAppFeedback appFeedback, CancellationToken cancellationToken)
    {
        return _dbContext.CustomerAppFeedbacks.AddAsync(appFeedback, cancellationToken).AsTask();
    }
}
