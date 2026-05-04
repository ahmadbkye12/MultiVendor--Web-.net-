using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Commands.PlaceOrder;

public sealed record PlaceOrderCommand(
    Guid ShippingAddressId,
    Guid? BillingAddressId,
    PaymentMethod PaymentMethod,
    string? CouponCode = null
) : IRequest<Result<Guid>>;

public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.ShippingAddressId).NotEmpty();
    }
}

public sealed class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    private readonly IDateTimeService _clock;
    private readonly IEmailService _email;
    private readonly IIdentityService _identity;

    public PlaceOrderCommandHandler(IApplicationDbContext db, ICurrentUserService user, IDateTimeService clock,
        IEmailService email, IIdentityService identity)
    {
        _db = db; _user = user; _clock = clock; _email = email; _identity = identity;
    }

    public async Task<Result<Guid>> Handle(PlaceOrderCommand req, CancellationToken ct)
    {
        var userId = _user.UserId ?? throw new ForbiddenAccessException();

        var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == req.ShippingAddressId, ct);
        if (address is null || address.UserId != userId)
            return Result<Guid>.Failure("Shipping address not found.");

        var cart = await _db.Carts
            .Include(c => c.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Product).ThenInclude(p => p.VendorStore).ThenInclude(s => s.Vendor)
            .FirstOrDefaultAsync(c => c.CustomerUserId == userId, ct);

        if (cart is null || !cart.Items.Any())
            return Result<Guid>.Failure("Your cart is empty.");

        // Re-validate stock and product status before committing.
        foreach (var i in cart.Items)
        {
            if (!i.ProductVariant.IsActive
                || !i.ProductVariant.Product.IsPublished
                || i.ProductVariant.Product.ApprovalStatus != ProductApprovalStatus.Approved)
                return Result<Guid>.Failure($"'{i.ProductVariant.Product.Name}' is no longer available.");

            if (i.ProductVariant.StockQuantity < i.Quantity)
                return Result<Guid>.Failure($"Not enough stock for '{i.ProductVariant.Product.Name}'.");
        }

        var now = _clock.UtcNow;
        var order = new Order
        {
            Id                = Guid.NewGuid(),   // pre-assigned so the OrderPlacedEvent captures the real id
            CustomerUserId    = userId,
            OrderNumber       = GenerateOrderNumber(now),
            ShippingAddressId = address.Id,
            BillingAddressId  = req.BillingAddressId ?? address.Id,
            ShippingFullName   = address.Label,
            ShippingPhone      = address.Phone,
            ShippingLine1      = address.Line1,
            ShippingLine2      = address.Line2,
            ShippingCity       = address.City,
            ShippingState      = address.State,
            ShippingPostalCode = address.PostalCode,
            ShippingCountry    = address.Country,
            Status      = OrderStatus.Paid,        // mocked checkout — payment always succeeds
            PlacedAtUtc = now,
            PaidAtUtc   = now,
            Subtotal       = 0m,
            ShippingAmount = 0m,
            TaxAmount      = 0m,
            DiscountAmount = 0m,
            Total          = 0m
        };

        decimal subtotal = 0m;
        foreach (var ci in cart.Items)
        {
            var v = ci.ProductVariant;
            var lineTotal = ci.UnitPrice * ci.Quantity;
            var commissionPct = v.Product.VendorStore.Vendor.DefaultCommissionPercent;
            var commissionAmt = Math.Round(lineTotal * commissionPct / 100m, 2);

            order.Items.Add(new OrderItem
            {
                ProductVariantId   = v.Id,
                VendorStoreId      = v.Product.VendorStoreId,
                ProductName        = v.Product.Name,
                VariantName        = v.Name ?? string.Join(" / ", new[] { v.Color, v.Size }.Where(s => !string.IsNullOrEmpty(s))),
                Quantity           = ci.Quantity,
                UnitPrice          = ci.UnitPrice,
                LineTotal          = lineTotal,
                CommissionPercent  = commissionPct,
                CommissionAmount   = commissionAmt,
                VendorNetAmount    = lineTotal - commissionAmt,
                VendorFulfillmentStatus = VendorOrderItemStatus.PendingFulfillment
            });

            v.StockQuantity -= ci.Quantity;
            subtotal += lineTotal;
        }

        order.Subtotal = subtotal;

        // Apply coupon if provided.
        if (!string.IsNullOrWhiteSpace(req.CouponCode))
        {
            var code = req.CouponCode.Trim().ToUpperInvariant();
            var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Code == code, ct);
            if (coupon is null) return Result<Guid>.Failure("Coupon not found.");
            if (!coupon.IsActive) return Result<Guid>.Failure("Coupon is not active.");
            if (coupon.StartsAtUtc.HasValue  && now < coupon.StartsAtUtc.Value)  return Result<Guid>.Failure("Coupon is not yet valid.");
            if (coupon.ExpiresAtUtc.HasValue && now > coupon.ExpiresAtUtc.Value) return Result<Guid>.Failure("Coupon has expired.");
            if (coupon.MaxUses.HasValue && coupon.UsedCount >= coupon.MaxUses.Value) return Result<Guid>.Failure("Coupon usage limit reached.");
            if (subtotal < coupon.MinimumOrderAmount) return Result<Guid>.Failure($"Minimum order amount {coupon.MinimumOrderAmount:0.00} not met.");

            if (coupon.MaxUsesPerCustomer.HasValue)
            {
                var myUses = await _db.Orders.CountAsync(o => o.CouponId == coupon.Id && o.CustomerUserId == userId, ct);
                if (myUses >= coupon.MaxUsesPerCustomer.Value)
                    return Result<Guid>.Failure("You have reached your usage limit for this coupon.");
            }

            decimal discount = coupon.DiscountType == CouponDiscountType.Percentage
                ? Math.Round(subtotal * coupon.DiscountValue / 100m, 2)
                : Math.Min(coupon.DiscountValue, subtotal);

            order.CouponId       = coupon.Id;
            order.DiscountAmount = discount;
            coupon.UsedCount    += 1;
        }

        order.Total = subtotal - order.DiscountAmount;        // tax/shipping = 0 in this academic build

        order.Payments.Add(new Payment
        {
            Amount   = order.Total,
            Status   = PaymentStatus.Captured,
            Provider = req.PaymentMethod.ToString()
        });

        // Clear the cart.
        _db.CartItems.RemoveRange(cart.Items);

        _db.Orders.Add(order);
        order.AddDomainEvent(new Domain.Events.OrderPlacedEvent(order.Id, order.OrderNumber, userId, order.Total));

        await _db.SaveChangesAsync(ct);

        // Fire-and-forget order confirmation email (logs to console in dev).
        var customer = await _identity.GetUserAsync(userId);
        if (customer is not null && !string.IsNullOrEmpty(customer.Email))
        {
            var body = $"<h3>Thanks for your order, {customer.FullName}!</h3>" +
                       $"<p>Order <strong>{order.OrderNumber}</strong> has been placed.</p>" +
                       $"<p>Total: <strong>{order.Total:0.00}</strong></p>";
            await _email.SendAsync(customer.Email, $"Order confirmation — {order.OrderNumber}", body, ct);
        }

        return Result<Guid>.Success(order.Id);
    }

    private static string GenerateOrderNumber(DateTime utcNow) =>
        $"ORD-{utcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
}
