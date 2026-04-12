using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface ICustomerAppRepository
{
    Task<IReadOnlyCollection<CustomerAddress>> ListAddressesAsync(long customerId, CancellationToken cancellationToken);

    Task<CustomerAddress?> GetAddressForUpdateAsync(long customerId, long customerAddressId, CancellationToken cancellationToken);

    Task AddAddressAsync(CustomerAddress customerAddress, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CustomerAddress>> ListDefaultAddressesForUpdateAsync(long customerId, long? excludedAddressId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CustomerEquipment>> ListEquipmentAsync(long customerId, CancellationToken cancellationToken);

    Task<CustomerEquipment?> GetEquipmentForUpdateAsync(long customerId, long customerEquipmentId, CancellationToken cancellationToken);

    Task AddEquipmentAsync(CustomerEquipment customerEquipment, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CustomerNotification>> ListNotificationsAsync(long customerId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<int> CountNotificationsAsync(long customerId, CancellationToken cancellationToken);

    Task<CustomerNotification?> GetNotificationForUpdateAsync(long customerId, long customerNotificationId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PromotionalOffer>> ListActiveOffersAsync(DateOnly currentDate, CancellationToken cancellationToken);

    Task<PromotionalOffer?> GetActiveOfferByCodeAsync(string offerCode, DateOnly currentDate, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CustomerReferral>> ListReferralsAsync(long customerId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CustomerLoyaltyTransaction>> ListLoyaltyTransactionsAsync(long customerId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CustomerReview>> ListReviewsAsync(long? serviceId, CancellationToken cancellationToken);

    Task AddReviewAsync(CustomerReview customerReview, CancellationToken cancellationToken);

    Task AddAppFeedbackAsync(CustomerAppFeedback appFeedback, CancellationToken cancellationToken);
}
