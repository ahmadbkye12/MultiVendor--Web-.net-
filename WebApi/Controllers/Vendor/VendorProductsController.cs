using Application.Authorization;
using Application.Vendor;
using Application.Vendor.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers.Vendor;

[ApiController]
[Route("api/vendor/products")]
[Authorize(Policy = AuthPolicies.ApprovedVendorOnly)]
public sealed class VendorProductsController(
    IVendorProductService products,
    IValidator<CreateVendorProductRequest> createValidator,
    IValidator<UpdateVendorProductRequest> updateValidator,
    IValidator<UpdateVariantStockRequest> stockValidator,
    IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VendorProductListItemDto>>> List([FromQuery] Guid? storeId, CancellationToken ct) =>
        Ok(await products.ListAsync(storeId, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VendorProductDetailDto>> Get(Guid id, CancellationToken ct)
    {
        var dto = await products.GetAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<VendorProductDetailDto>> Create([FromBody] CreateVendorProductRequest request, CancellationToken ct)
    {
        var vr = await createValidator.ValidateAsync(request, ct);
        if (!vr.IsValid)
            return ValidationProblem(vr.ToValidationProblemDetails());

        try
        {
            var created = await products.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VendorProductDetailDto>> Update(Guid id, [FromBody] UpdateVendorProductRequest request, CancellationToken ct)
    {
        var vr = await updateValidator.ValidateAsync(request, ct);
        if (!vr.IsValid)
            return ValidationProblem(vr.ToValidationProblemDetails());

        try
        {
            var dto = await products.UpdateAsync(id, request, ct);
            return dto is null ? NotFound() : Ok(dto);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var ok = await products.SoftDeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/images")]
    public async Task<ActionResult<VendorProductDetailDto>> AddImageUrl(Guid id, [FromBody] AddImageUrlBody body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.Url) || body.Url.Length > 2048)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["url"] = ["URL is required (max 2048 characters)."] }));

        var dto = await products.AddImageAsync(id, body.Url, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{id:guid}/images/upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<VendorProductDetailDto>> UploadImage(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0 || file.Length > 6 * 1024 * 1024)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["file"] = ["Image required (max 6 MB)."] }));

        var relative = await VendorUploads.SaveProductImageAsync(environment, id, file, ct);
        if (relative is null)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["file"] = ["Allowed types: jpg, jpeg, png, webp, gif."] }));

        var dto = await products.AddImageAsync(id, relative, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpDelete("{productId:guid}/images/{imageId:guid}")]
    public async Task<ActionResult<VendorProductDetailDto>> DeleteImage(Guid productId, Guid imageId, CancellationToken ct)
    {
        var ok = await products.SoftDeleteImageAsync(productId, imageId, ct);
        if (!ok)
            return NotFound();

        var dto = await products.GetAsync(productId, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPatch("{productId:guid}/variants/{variantId:guid}/stock")]
    public async Task<ActionResult<VendorProductDetailDto>> PatchStock(
        Guid productId,
        Guid variantId,
        [FromBody] UpdateVariantStockRequest request,
        CancellationToken ct)
    {
        var vr = await stockValidator.ValidateAsync(request, ct);
        if (!vr.IsValid)
            return ValidationProblem(vr.ToValidationProblemDetails());

        var dto = await products.UpdateVariantStockAsync(productId, variantId, request.StockQuantity, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    public sealed record AddImageUrlBody(string Url);
}
