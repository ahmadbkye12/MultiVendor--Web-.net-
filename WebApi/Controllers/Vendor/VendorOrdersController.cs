using Application.Authorization;
using Application.Vendor;
using Application.Vendor.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers.Vendor;

[ApiController]
[Route("api/vendor/orders")]
[Authorize(Policy = AuthPolicies.ApprovedVendorOnly)]
public sealed class VendorOrdersController(
    IVendorOrderService orders,
    IValidator<UpdateVendorOrderItemStatusRequest> statusValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VendorOrderListItemDto>>> List(CancellationToken ct) =>
        Ok(await orders.ListAsync(ct));

    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<VendorOrderDetailDto>> Get(Guid orderId, CancellationToken ct)
    {
        var dto = await orders.GetAsync(orderId, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPatch("{orderId:guid}/items/{itemId:guid}/status")]
    public async Task<ActionResult<VendorOrderDetailDto>> UpdateStatus(
        Guid orderId,
        Guid itemId,
        [FromBody] UpdateVendorOrderItemStatusRequest request,
        CancellationToken ct)
    {
        var vr = await statusValidator.ValidateAsync(request, ct);
        if (!vr.IsValid)
            return ValidationProblem(vr.ToValidationProblemDetails());

        var dto = await orders.UpdateItemStatusAsync(orderId, itemId, request.Status, ct);
        return dto is null ? NotFound() : Ok(dto);
    }
}
