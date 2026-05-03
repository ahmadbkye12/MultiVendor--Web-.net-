using Application.Authorization;
using Application.Vendor;
using Application.Vendor.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers.Vendor;

[ApiController]
[Route("api/vendor/stores")]
[Authorize(Policy = AuthPolicies.ApprovedVendorOnly)]
public sealed class VendorStoresController(
    IVendorStoreService stores,
    IValidator<UpdateVendorStoreRequest> updateValidator,
    IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VendorStoreSummaryDto>>> List(CancellationToken ct) =>
        Ok(await stores.ListStoresAsync(ct));

    [HttpPut("{storeId:guid}")]
    public async Task<ActionResult<VendorStoreSummaryDto>> Update(Guid storeId, [FromBody] UpdateVendorStoreRequest request, CancellationToken ct)
    {
        var vr = await updateValidator.ValidateAsync(request, ct);
        if (!vr.IsValid)
            return ValidationProblem(vr.ToValidationProblemDetails());

        var dto = await stores.UpdateStoreAsync(storeId, request, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{storeId:guid}/logo/upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<VendorStoreSummaryDto>> UploadLogo(Guid storeId, IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0 || file.Length > 5 * 1024 * 1024)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["file"] = ["Logo required (max 5 MB)."] }));

        var relative = await VendorUploads.SaveStoreAssetAsync(environment, storeId, "logo", file, ct);
        if (relative is null)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["file"] = ["Allowed types: jpg, jpeg, png, webp, gif."] }));

        var current = await stores.ListStoresAsync(ct);
        var existing = current.FirstOrDefault(s => s.Id == storeId);
        if (existing is null)
            return NotFound();

        var dto = await stores.UpdateStoreAsync(
            storeId,
            new UpdateVendorStoreRequest(
                Name: existing.Name,
                Description: existing.Description,
                ContactEmail: existing.ContactEmail,
                ContactPhone: existing.ContactPhone,
                IsActive: existing.IsActive,
                LogoUrl: relative,
                BannerUrl: existing.BannerUrl),
            ct);

        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{storeId:guid}/banner/upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(15 * 1024 * 1024)]
    public async Task<ActionResult<VendorStoreSummaryDto>> UploadBanner(Guid storeId, IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0 || file.Length > 12 * 1024 * 1024)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["file"] = ["Banner required (max 12 MB)."] }));

        var relative = await VendorUploads.SaveStoreAssetAsync(environment, storeId, "banner", file, ct);
        if (relative is null)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["file"] = ["Allowed types: jpg, jpeg, png, webp, gif."] }));

        var current = await stores.ListStoresAsync(ct);
        var existing = current.FirstOrDefault(s => s.Id == storeId);
        if (existing is null)
            return NotFound();

        var dto = await stores.UpdateStoreAsync(
            storeId,
            new UpdateVendorStoreRequest(
                Name: existing.Name,
                Description: existing.Description,
                ContactEmail: existing.ContactEmail,
                ContactPhone: existing.ContactPhone,
                IsActive: existing.IsActive,
                LogoUrl: existing.LogoUrl,
                BannerUrl: relative),
            ct);

        return dto is null ? NotFound() : Ok(dto);
    }
}
