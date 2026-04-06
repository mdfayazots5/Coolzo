using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface IBookingRepository
{
    Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken);

    Task AddCustomerAddressAsync(CustomerAddress customerAddress, CancellationToken cancellationToken);

    Task AddBookingAsync(Booking booking, CancellationToken cancellationToken);

    Task<bool> BookingReferenceExistsAsync(string bookingReference, CancellationToken cancellationToken);

    Task<Customer?> GetCustomerByIdAsync(long customerId, CancellationToken cancellationToken);

    Task<Customer?> GetCustomerByUserIdAsync(long userId, CancellationToken cancellationToken);

    Task<Customer?> GetCustomerByMobileAsync(string mobileNumber, CancellationToken cancellationToken);

    Task<CustomerAddress?> GetCustomerAddressAsync(long customerId, string addressLine1, string pincode, CancellationToken cancellationToken);

    Task<Booking?> GetByIdAsync(long bookingId, CancellationToken cancellationToken);

    Task<Booking?> GetByIdForUpdateAsync(long bookingId, CancellationToken cancellationToken);

    Task<Booking?> GetByIdForCustomerAsync(long bookingId, long customerId, CancellationToken cancellationToken);

    Task<Booking?> GetByIdForCustomerForUpdateAsync(long bookingId, long customerId, CancellationToken cancellationToken);

    Task<Booking?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken);

    Task<bool> HasDuplicateBookingAsync(string mobileNumber, long slotAvailabilityId, long serviceId, CancellationToken cancellationToken);

    Task<bool> HasSlotConflictAsync(string mobileNumber, DateOnly slotDate, long slotConfigurationId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Booking>> ListByCustomerIdAsync(long customerId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<int> CountByCustomerIdAsync(long customerId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Booking>> SearchAsync(
        string? bookingReference,
        string? customerMobile,
        DateOnly? bookingDate,
        long? serviceId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> CountSearchAsync(
        string? bookingReference,
        string? customerMobile,
        DateOnly? bookingDate,
        long? serviceId,
        CancellationToken cancellationToken);
}
