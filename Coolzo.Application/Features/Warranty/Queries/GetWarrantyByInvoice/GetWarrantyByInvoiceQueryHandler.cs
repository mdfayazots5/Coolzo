using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Features.Amc;
using Coolzo.Contracts.Responses.Warranty;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.Warranty.Queries.GetWarrantyByInvoice;

public sealed class GetWarrantyByInvoiceQueryHandler : IRequestHandler<GetWarrantyByInvoiceQuery, WarrantyStatusResponse>
{
    private readonly IAmcRepository _amcRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ServiceLifecycleAccessService _serviceLifecycleAccessService;

    public GetWarrantyByInvoiceQueryHandler(
        IAmcRepository amcRepository,
        ICurrentDateTime currentDateTime,
        ServiceLifecycleAccessService serviceLifecycleAccessService)
    {
        _amcRepository = amcRepository;
        _currentDateTime = currentDateTime;
        _serviceLifecycleAccessService = serviceLifecycleAccessService;
    }

    public async Task<WarrantyStatusResponse> Handle(GetWarrantyByInvoiceQuery request, CancellationToken cancellationToken)
    {
        var invoiceHeader = await _amcRepository.GetInvoiceByIdAsync(request.InvoiceId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "The requested invoice could not be found.", 404);

        await _serviceLifecycleAccessService.EnsureInvoiceReadAccessAsync(invoiceHeader, cancellationToken);

        var primaryLine = WarrantyEligibilityHelper.GetPrimaryBookingLine(invoiceHeader);
        var warrantyRule = primaryLine is null
            ? null
            : await _amcRepository.GetMatchingWarrantyRuleAsync(primaryLine.ServiceId, primaryLine.AcTypeId, primaryLine.BrandId, cancellationToken);
        var claims = await _amcRepository.GetWarrantyClaimsByInvoiceIdAsync(invoiceHeader.InvoiceHeaderId, cancellationToken);

        if (warrantyRule is null)
        {
            return WarrantyResponseMapper.ToStatus(
                invoiceHeader,
                null,
                null,
                null,
                false,
                "No active warranty rule is configured for this billed service.",
                claims);
        }

        var (coverageStartDateUtc, coverageEndDateUtc, isEligible) = WarrantyEligibilityHelper.EvaluateCoverage(
            invoiceHeader,
            warrantyRule,
            _currentDateTime.UtcNow);
        var eligibilityMessage = isEligible
            ? "Warranty is active for the selected invoice."
            : "Warranty coverage has expired for the selected invoice.";

        return WarrantyResponseMapper.ToStatus(
            invoiceHeader,
            warrantyRule,
            coverageStartDateUtc,
            coverageEndDateUtc,
            isEligible,
            eligibilityMessage,
            claims);
    }
}
