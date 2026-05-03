using Domain.Enums;
using FluentValidation;

namespace Application.Vendor.Validation;

public sealed class ProductVariantUpsertDtoValidator : AbstractValidator<ProductVariantUpsertDto>
{
    public ProductVariantUpsertDtoValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Name).MaximumLength(256).When(x => x.Name != null);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateVendorProductRequestValidator : AbstractValidator<CreateVendorProductRequest>
{
    public CreateVendorProductRequestValidator()
    {
        RuleFor(x => x.VendorStoreId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Description).MaximumLength(8000).When(x => x.Description != null);
        RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Slug).MaximumLength(512).When(x => x.Slug != null);
        RuleFor(x => x.Variants).NotEmpty().Must(v => v.Count <= 50).WithMessage("A maximum of 50 variants is allowed.");
        RuleForEach(x => x.Variants).SetValidator(new ProductVariantUpsertDtoValidator());
        RuleFor(x => x.Variants)
            .Must(v => v.Select(x => x.Sku.ToUpperInvariant()).Distinct().Count() == v.Count)
            .WithMessage("Variant SKUs must be unique.");
    }
}

public sealed class UpdateVendorProductRequestValidator : AbstractValidator<UpdateVendorProductRequest>
{
    public UpdateVendorProductRequestValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Description).MaximumLength(8000).When(x => x.Description != null);
        RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Slug).MaximumLength(512).When(x => x.Slug != null);
    }
}

public sealed class UpdateVariantStockRequestValidator : AbstractValidator<UpdateVariantStockRequest>
{
    public UpdateVariantStockRequestValidator()
    {
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateVendorStoreRequestValidator : AbstractValidator<UpdateVendorStoreRequest>
{
    public UpdateVendorStoreRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(4000).When(x => x.Description != null);
        RuleFor(x => x.ContactEmail)
            .EmailAddress()
            .MaximumLength(256)
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
        RuleFor(x => x.ContactPhone).MaximumLength(64).When(x => x.ContactPhone != null);
        RuleFor(x => x.LogoUrl).MaximumLength(2048).When(x => x.LogoUrl != null);
        RuleFor(x => x.BannerUrl).MaximumLength(2048).When(x => x.BannerUrl != null);
    }
}

public sealed class UpdateVendorOrderItemStatusRequestValidator : AbstractValidator<UpdateVendorOrderItemStatusRequest>
{
    public UpdateVendorOrderItemStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
