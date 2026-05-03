using Application.Authorization;
using Application.Vendor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Vendor;

[ApiController]
[Route("api/vendor/earnings")]
[Authorize(Policy = AuthPolicies.ApprovedVendorOnly)]
public sealed class VendorEarningsController(IVendorEarningsService earnings) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<VendorEarningsSummaryDto>> Summary(CancellationToken ct) =>
        Ok(await earnings.GetSummaryAsync(ct));

    [HttpGet("lines")]
    public async Task<ActionResult<IReadOnlyList<VendorEarningLineDto>>> Lines(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        CancellationToken ct = default) =>
        Ok(await earnings.ListLinesAsync(skip, take, ct));
}
