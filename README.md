# MultiVendor E-Commerce Marketplace

A multi-vendor marketplace built on **ASP.NET Core MVC 8** with **Clean Architecture**. Customers shop across many vendor stores; each vendor manages their own storefront, products, fulfillment, and coupons; admins gate the marketplace via approvals and moderation.

> **Architecture diagram, entity model, and design rationale:** see [PROJECT_ARCHITECTURE.md](PROJECT_ARCHITECTURE.md).

---

## Quick start

### Prerequisites
- .NET 8 SDK
- SQL Server LocalDB (ships with Visual Studio / installable as a standalone)

### Run
```bash
git clone <repo-url> MultiVendorEcommerce
cd MultiVendorEcommerce

# Apply migrations and seed (idempotent — also runs on first app startup)
dotnet ef database update \
  --project src/MultiVendorEcommerce.Infrastructure \
  --startup-project src/MultiVendorEcommerce.Web

# Run
dotnet run --project src/MultiVendorEcommerce.Web --launch-profile http
```

Open: <http://localhost:5244>

### Default credentials (auto-seeded)

| Role | Email | Password |
|---|---|---|
| **Admin** | `admin@shop.com` | `Admin123!` |
| **Vendor** (TechGear) | `techgear@shop.com` | `Demo123!` |
| **Vendor** (StyleHub) | `stylehub@shop.com` | `Demo123!` |
| **Customer** | `demo@shop.com` | `Demo123!` |

**Sample coupon:** `WELCOME10` — 10% off any order.

---

## What you'll see

- 5 categories, 10 seeded products across 2 storefronts (TechGear / StyleHub) — homepage shows them as Featured products
- Browse → Product details → Add to cart → Checkout → Place order → Download PDF invoice
- Admin: approve vendors, approve products, manage categories, moderate reviews, view audit log, handle refund requests, run platform-wide coupons
- Vendor: store settings, products CRUD with images + variants, low-stock list, orders + per-line fulfillment, shipments, vendor coupons, customer list, reviews + reply, sales chart
- Live SignalR toasts when a refund decision lands or a review is submitted
- Toggle between English and Türkçe via the navbar

---

## Architecture

**Clean Architecture, four layers — dependencies point inward only:**

```
Web (ASP.NET Core MVC)
  ↓
Infrastructure (EF Core, Identity, FileStorage, SignalR, AuditLogger)
  ↓
Application (CQRS via MediatR, FluentValidation, AutoMapper, behaviors)
  ↓
Domain (entities, enums, BaseAuditableEntity, domain events)
```

**Cross-cutting patterns:**
- CQRS with MediatR (every use case = `IRequest<T>` + `IRequestHandler<T,R>`)
- MediatR pipeline: `ValidationBehaviour`, `UnhandledExceptionBehaviour`
- EF Core interceptors: `AuditableEntityInterceptor` (CreatedBy/UpdatedBy + soft-delete), `DispatchDomainEventsInterceptor`
- Result pattern (`Result<T>`) for predictable success/failure
- Resource ownership enforced inside handlers (`ICurrentUserService.UserId` + `Vendor.OwnerUserId` filter)

For the full breakdown of entities, workflows, and folder structure see [PROJECT_DESIGN.md](PROJECT_DESIGN.md).

---

## Tech stack

| Concern | Choice |
|---|---|
| Framework | ASP.NET Core MVC 8.0 |
| ORM | EF Core 8 (Code-First) on SQL Server LocalDB |
| Mediator | MediatR 12 |
| Validation | FluentValidation 11 + DataAnnotations |
| Mapping | AutoMapper 14 |
| Auth | ASP.NET Core Identity (cookie-based) |
| UI | Razor Views + Bootstrap 5 + AdminLTE 4 + Bootstrap Icons |
| Charts | Chart.js (CDN) |
| PDF | QuestPDF |
| Real-time | ASP.NET Core SignalR |
| Localization | `IStringLocalizer` + `.resx` (en / tr) |
| Email | `IEmailService` + `ConsoleEmailService` (logs to console; swap for SMTP in production) |

---

## Pages (30+ shipped)

### Public / Customer
Home · Login · Register · Register as Vendor · Product list · Product details · Vendor storefront · Cart · Checkout · Order history · Order details (with status timeline) · Wishlist · Profile · Change password · Notifications · Addresses

### Vendor area
Dashboard (with sales chart) · My products · Low stock · Orders · Shipments · My customers · Reviews · Coupons · Store settings

### Admin area
Dashboard (with combo chart + KPIs) · Categories · Vendors (approve / set commission) · Products approval · Users (role editor) · Coupons (platform) · Reviews (moderation) · Refund requests · Audit log

---

## Project rules satisfied (from the brief)

| Rule | Where |
|---|---|
| ≥ 10 pages | 30+ shipped |
| DB connection | EF Core + SQL Server in `appsettings.json` |
| Code-First | Migrations in `Infrastructure/Persistence/Migrations` |
| CRUD (add/list/update/delete/details) | Categories, Products (with variants + images), Orders, Shipments, Coupons, Reviews, Users, Vendors, Addresses, Wishlist |
| Username/password login | ASP.NET Core Identity at `/Account/Login` |
| ≥ 2 layouts | 3: `_PublicLayout`, `_AdminLayout`, `_AuthLayout` |
| ViewModels & validation | DataAnnotations on ViewModels + FluentValidation on commands |
| ViewBag / ViewData / TempData / ViewModel all used | Mapped throughout: e.g. ViewBag for SelectLists, ViewData for KPI tiles, TempData for redirect toasts |

---

## Project layout

```
MultiVendorEcommerce.sln
├── src/
│   ├── MultiVendorEcommerce.Domain/          Entities, enums, base classes, domain events
│   ├── MultiVendorEcommerce.Application/     CQRS handlers, DTOs, validators, abstractions
│   ├── MultiVendorEcommerce.Infrastructure/  DbContext, Identity, services, interceptors, seed
│   └── MultiVendorEcommerce.Web/             Controllers, Views, ViewModels, Areas (Admin / Vendor)
└── tests/                                    (xUnit projects)
```

---

## Configuration

`src/MultiVendorEcommerce.Web/appsettings.json` — connection string only.

```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\mssqllocaldb;Database=MultiVendorEcommerceDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

The app calls `MigrateAsync()` + `DbInitializer` on startup, so you don't need to run `dotnet ef database update` manually after the first time — though you can.

---

## Demo flow you can click in 60 seconds

1. Open <http://localhost:5244> — featured products from both stores
2. Click **TechGear** on any card → store front
3. Login as `demo@shop.com` / `Demo123!`
4. Add a product to cart → Checkout
5. Apply coupon `WELCOME10` → place order
6. **Download invoice** as PDF from order details
7. Switch the language dropdown to **Türkçe** → entire navbar + home re-renders
8. Logout, login as `admin@shop.com` / `Admin123!`
9. Admin Dashboard shows the new order in the chart
10. Visit `/Admin/AuditLogs` to see the login/logout entries

---

## License

Academic project — no production warranty.
