using Application.Authorization;
using Application.Vendor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Vendor;

[ApiController]
[Route("api/vendor/reviews")]
[Authorize(Policy = AuthPolicies.ApprovedVendorOnly)]
public sealed class VendorReviewsController(IVendorReviewService reviews) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VendorReviewListItemDto>>> List(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default) =>
        Ok(await reviews.ListAsync(skip, take, cancellationToken));
}
