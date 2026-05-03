using Application.Authorization;
using Application.Vendor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Vendor;

[ApiController]
[Route("api/vendor/dashboard")]
[Authorize(Policy = AuthPolicies.ApprovedVendorOnly)]
public sealed class VendorDashboardController(IVendorDashboardService dashboard) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<VendorDashboardSummaryDto>> Summary(CancellationToken cancellationToken) =>
        Ok(await dashboard.GetSummaryAsync(cancellationToken));
}
