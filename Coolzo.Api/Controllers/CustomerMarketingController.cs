using Asp.Versioning;
using Coolzo.Application.Features.CustomerApp;
using Coolzo.Contracts.Common;
using Coolzo.Contracts.Requests.Customer;
using Coolzo.Contracts.Responses.Customer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coolzo.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
public sealed class CustomerMarketingController : ApiControllerBase
{
    private readonly ISender _sender;

    public CustomerMarketingController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("offers")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<PromotionalOfferResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<PromotionalOfferResponse>>>> GetOffersAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetActivePromotionalOffersQuery(), cancellationToken);
        return Success(response);
    }

    [Authorize]
    [HttpPost("offers/validate-coupon")]
    [ProducesResponseType(typeof(ApiResponse<PromotionalOfferResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PromotionalOfferResponse?>>> ValidateCouponAsync(
        [FromBody] ValidateCouponRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new ValidateCouponQuery(request.Code), cancellationToken);
        return Success(response, response is null ? "Coupon is not valid." : "Coupon validated successfully.");
    }

    [Authorize]
    [HttpGet("referrals/me")]
    [ProducesResponseType(typeof(ApiResponse<ReferralStatsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ReferralStatsResponse>>> GetMyReferralStatsAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetMyReferralStatsQuery(), cancellationToken);
        return Success(response);
    }

    [Authorize]
    [HttpGet("loyalty/me")]
    [ProducesResponseType(typeof(ApiResponse<LoyaltyPointsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LoyaltyPointsResponse>>> GetMyLoyaltyPointsAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetMyLoyaltyPointsQuery(), cancellationToken);
        return Success(response);
    }

    [Authorize]
    [HttpGet("loyalty/me/transactions")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<LoyaltyTransactionResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<LoyaltyTransactionResponse>>>> GetMyLoyaltyTransactionsAsync(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetMyLoyaltyTransactionsQuery(), cancellationToken);
        return Success(response);
    }
}
