namespace WebApi.Controllers.Vendor;

public static class VendorUploads
{
    private static readonly HashSet<string> AllowedExt = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    public static async Task<string?> SaveProductImageAsync(
        IWebHostEnvironment env,
        Guid productId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var ext = Path.GetExtension(file.FileName);
        if (!AllowedExt.Contains(ext))
            return null;

        var root = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var dir = Path.Combine(root, "uploads", "products", productId.ToString());
        Directory.CreateDirectory(dir);

        var name = $"{Guid.CreateVersion7():N}{ext}";
        var physical = Path.Combine(dir, name);
        await using (var fs = File.Create(physical))
            await file.CopyToAsync(fs, cancellationToken);

        return $"/uploads/products/{productId}/{name}";
    }

    public static async Task<string?> SaveStoreAssetAsync(
        IWebHostEnvironment env,
        Guid storeId,
        string kind,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var ext = Path.GetExtension(file.FileName);
        if (!AllowedExt.Contains(ext))
            return null;

        var root = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var dir = Path.Combine(root, "uploads", "stores", storeId.ToString(), kind);
        Directory.CreateDirectory(dir);

        var name = $"{Guid.CreateVersion7():N}{ext}";
        var physical = Path.Combine(dir, name);
        await using (var fs = File.Create(physical))
            await file.CopyToAsync(fs, cancellationToken);

        return $"/uploads/stores/{storeId}/{kind}/{name}";
    }
}
