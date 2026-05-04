# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Run the application
dotnet run --project src/MultiVendorEcommerce.Web --launch-profile http
# Opens at http://localhost:5244

# Apply EF Core migrations manually (also runs automatically on startup)
dotnet ef database update \
  --project src/MultiVendorEcommerce.Infrastructure \
  --startup-project src/MultiVendorEcommerce.Web

# Add a new migration
dotnet ef migrations add <MigrationName> \
  --project src/MultiVendorEcommerce.Infrastructure \
  --startup-project src/MultiVendorEcommerce.Web

# Run all tests
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~PlaceOrderCommandHandlerTests"
```

Default seeded credentials: Admin `admin@shop.com` / `Admin123!`, Vendor `techgear@shop.com` / `Demo123!`, Customer `demo@shop.com` / `Demo123!`. Sample coupon: `WELCOME10`.

## Architecture

Clean Architecture — four layers, dependencies point inward only:

```
Web (ASP.NET Core MVC) → Infrastructure (EF Core, Identity, Services) → Application (CQRS) → Domain (entities, enums, events)
```

**Domain** (`MultiVendorEcommerce.Domain`) — pure C# with no framework dependencies. All entities inherit `BaseAuditableEntity` (Guid Id, CreatedBy/UpdatedBy/DeletedBy + `IsDeleted` for soft-delete). Eight enums cover order/payment/shipment/fulfillment/approval/coupon/notification/audit states.

**Application** (`MultiVendorEcommerce.Application`) — references Domain only. Every use case is an `IRequest<Result<T>>` + `IRequestHandler`. Feature folders: `Vendors/`, `VendorStores/`, `Products/`, `Categories/`, `Cart/`, `Orders/`, `Shipments/`, `Payments/`, `Coupons/`, `Reviews/`, `Wishlist/`, `Addresses/`, `Notifications/`, `AuditLogs/`, `Identity/`. Interfaces defined here, implemented in Infrastructure: `IApplicationDbContext`, `ICurrentUserService`, `IIdentityService`, `IFileStorageService`, `IEmailService`, `IDateTime`.

**Infrastructure** (`MultiVendorEcommerce.Infrastructure`) — EF Core `ApplicationDbContext` (also `IdentityDbContext<ApplicationUser>`), Fluent API configs in `Persistence/Configurations/`, interceptors (`AuditableEntityInterceptor` for audit stamps, `DispatchDomainEventsInterceptor`), `DbInitializer` seed, and concrete service implementations.

**Web** (`MultiVendorEcommerce.Web`) — thin controllers that dispatch to MediatR and map ViewModels ↔ Commands. Admin and Vendor functionality is isolated in `Areas/Admin/` and `Areas/Vendor/`. Three layouts: `_PublicLayout`, `_AdminLayout` (shared by Admin + Vendor areas), `_AuthLayout`.

## Key Patterns

**CQRS via MediatR** — controllers call `_mediator.Send(command)` and receive `Result<T>`. Never put business logic in controllers.

**Result pattern** — `Result<T>.Success(value)` / `Result<T>.Failure("message")`. Handlers return failures; controllers check `result.IsSuccess` and add `ModelState` errors or redirect with `TempData`.

**Two-layer validation** — DataAnnotations on ViewModels drive jQuery client-side validation; FluentValidation on Commands runs server-side via `ValidationBehaviour` in the MediatR pipeline. Pipeline also includes `UnhandledExceptionBehaviour`, `LoggingBehaviour`, `PerformanceBehaviour` (warns > 500ms).

**Resource ownership in handlers** — vendor-area handlers resolve the current user's stores via `ICurrentUserService.UserId` and filter by `VendorStoreId`. Never trust IDs from the URL to authorize access; always verify the relationship inside the handler.

**Soft delete** — `IsDeleted` is filtered globally by EF Core query filters on all entities. Restoration is flipping the flag.

## Domain Design Notes

- Cart and Order line items reference `ProductVariant` (not `Product`) — variants hold price and stock.
- `OrderItem` is a snapshot: `ProductName`, `VariantName`, `UnitPrice`, and commission fields (`CommissionPercent`, `CommissionAmount`, `VendorNetAmount`) are copied at checkout time and never mutated.
- `Order` copies the selected `Address` fields onto itself (`ShippingFullName`, `ShippingLine1`, …) so address edits don't corrupt historical orders.
- `Coupon.VendorStoreId == null` means platform-wide (admin-managed); non-null means vendor-scoped.
- Multi-vendor orders produce one `OrderItem` per vendor line and one `Shipment` per vendor — each vendor fulfills and ships independently.
- `PlaceOrderCommand` (in `Orders/`) contains the payment flow inline — there is no separate `Payments/` handler for order placement.
- `DispatchDomainEventsInterceptor` and `BaseDomainEvent` exist; `OrderPlacedEvent` is the primary domain event (triggers `OrderPlacedAuditHandler`).

## Testing

Single test project: `tests/MultiVendorEcommerce.Application.UnitTests` (xUnit + EF InMemory + FluentAssertions + Moq). Tests cover `PlaceOrderCommandHandler` and `CreateReviewCommandHandler`. Integration tests project exists but is empty.

## Tech Stack Summary

ASP.NET Core MVC 8, C# 12, EF Core 8 on SQL Server LocalDB, MediatR 12, FluentValidation 11, AutoMapper 14, ASP.NET Core Identity (cookie-based), SignalR, QuestPDF, Chart.js, Bootstrap 5 + AdminLTE 4, `IStringLocalizer` for en/tr localization.
