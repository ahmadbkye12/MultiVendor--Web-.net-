using Application.Common;
using Application.Vendor;
using Application.Vendor.Validation;

namespace UnitTests;

public class VendorCommissionCalculatorTests
{
    [Theory]
    [InlineData(100, 10, 10, 90)]
    [InlineData(33.33, 15, 5.00, 28.33)]
    public void Split_matches_expected(decimal lineTotal, decimal percent, decimal expectedCommission, decimal expectedNet)
    {
        var (c, n) = VendorCommissionCalculator.Split(lineTotal, percent);
        Assert.Equal(expectedCommission, c);
        Assert.Equal(expectedNet, n);
    }
}

public class SlugHelperTests
{
    [Theory]
    [InlineData("Demo Phone X", "demo-phone-x")]
    [InlineData("  Hello   ", "hello")]
    [InlineData("___", "item")]
    public void Slugify_normalizes(string input, string expected) =>
        Assert.Equal(expected, SlugHelper.Slugify(input));
}

public class CreateVendorProductRequestValidatorTests
{
    private readonly CreateVendorProductRequestValidator _validator = new();

    [Fact]
    public void Valid_request_passes()
    {
        var model = new CreateVendorProductRequest(
            VendorStoreId: Guid.NewGuid(),
            CategoryId: Guid.NewGuid(),
            Name: "Widget",
            Description: "Nice",
            BasePrice: 10,
            IsPublished: false,
            Slug: null,
            Variants: [new ProductVariantUpsertDto("SKU-1", "Small", 10, 5)]);

        var result = _validator.Validate(model);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Duplicate_variant_skus_fail()
    {
        var model = new CreateVendorProductRequest(
            VendorStoreId: Guid.NewGuid(),
            CategoryId: Guid.NewGuid(),
            Name: "Widget",
            Description: null,
            BasePrice: 10,
            IsPublished: false,
            Slug: null,
            Variants:
            [
                new ProductVariantUpsertDto("SKU-A", null, 10, 1),
                new ProductVariantUpsertDto("sku-a", null, 11, 2)
            ]);

        var result = _validator.Validate(model);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateVendorProductRequest.Variants));
    }

    [Fact]
    public void Empty_variants_fail()
    {
        var model = new CreateVendorProductRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "W",
            null,
            1,
            false,
            null,
            Array.Empty<ProductVariantUpsertDto>());

        var result = _validator.Validate(model);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateVendorProductRequest.Variants));
    }
}
