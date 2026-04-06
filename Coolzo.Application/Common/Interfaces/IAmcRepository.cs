using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface IAmcRepository
{
    Task AddAmcPlanAsync(AmcPlan amcPlan, CancellationToken cancellationToken);

    Task<bool> AmcPlanNameExistsAsync(string planName, CancellationToken cancellationToken);

    Task<AmcPlan?> GetAmcPlanByIdAsync(long amcPlanId, CancellationToken cancellationToken);

    Task<AmcPlan?> GetAmcPlanByIdForUpdateAsync(long amcPlanId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<AmcPlan>> SearchAmcPlansAsync(
        bool? isActive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> CountAmcPlansAsync(bool? isActive, CancellationToken cancellationToken);

    Task<Customer?> GetCustomerByIdAsync(long customerId, CancellationToken cancellationToken);

    Task<JobCard?> GetJobCardByIdAsync(long jobCardId, CancellationToken cancellationToken);

    Task<InvoiceHeader?> GetInvoiceByIdAsync(long invoiceId, CancellationToken cancellationToken);

    Task AddCustomerAmcAsync(CustomerAmc customerAmc, CancellationToken cancellationToken);

    Task<CustomerAmc?> GetCustomerAmcByIdAsync(long customerAmcId, CancellationToken cancellationToken);

    Task<CustomerAmc?> GetCustomerAmcByIdForUpdateAsync(long customerAmcId, CancellationToken cancellationToken);

    Task<bool> HasActiveCustomerAmcAsync(
        long customerId,
        long amcPlanId,
        DateTime coverageDateUtc,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CustomerAmc>> GetCustomerAmcByCustomerIdAsync(long customerId, CancellationToken cancellationToken);

    Task AddAmcVisitScheduleAsync(AmcVisitSchedule amcVisitSchedule, CancellationToken cancellationToken);

    Task<bool> HasAmcVisitSchedulesAsync(long customerAmcId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<AmcVisitSchedule>> GetAmcVisitSchedulesByCustomerAmcIdAsync(
        long customerAmcId,
        CancellationToken cancellationToken);

    Task<WarrantyRule?> GetMatchingWarrantyRuleAsync(
        long serviceId,
        long? acTypeId,
        long? brandId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<WarrantyClaim>> GetWarrantyClaimsByInvoiceIdAsync(long invoiceId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<WarrantyClaim>> GetWarrantyClaimsByCustomerIdAsync(long customerId, CancellationToken cancellationToken);

    Task<bool> HasOpenWarrantyClaimAsync(long invoiceId, CancellationToken cancellationToken);

    Task AddWarrantyClaimAsync(WarrantyClaim warrantyClaim, CancellationToken cancellationToken);

    Task<WarrantyClaim?> GetWarrantyClaimByIdAsync(long warrantyClaimId, CancellationToken cancellationToken);

    Task<Booking?> GetBookingByIdAsync(long bookingId, CancellationToken cancellationToken);

    Task<ServiceRequest?> GetServiceRequestByIdAsync(long serviceRequestId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Booking>> GetBookingsByCustomerIdAsync(long customerId, CancellationToken cancellationToken);

    Task AddRevisitRequestAsync(RevisitRequest revisitRequest, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RevisitRequest>> GetRevisitRequestsByBookingIdAsync(long bookingId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RevisitRequest>> GetRevisitRequestsByCustomerIdAsync(long customerId, CancellationToken cancellationToken);

    Task<AmcVisitSchedule?> GetLinkedAmcVisitByServiceRequestIdAsync(long serviceRequestId, CancellationToken cancellationToken);

    Task<RevisitRequest?> GetLinkedRevisitByServiceRequestIdAsync(long serviceRequestId, CancellationToken cancellationToken);
}
