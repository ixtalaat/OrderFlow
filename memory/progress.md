# Project Progress

## Epic 7 — Background Processing

Configured Hangfire with SQL Server storage and a background job scheduler abstraction. Order creation queues notification jobs and order completion queues accounting synchronization jobs after successful persistence. Jobs use structured logging, three automatic retries, failed-state retention, and a role-protected `/hangfire` dashboard. SQLite test environments use a no-op scheduler.

## Epic 6 — Customer-Specific Pricing

Implemented Regular, Wholesale, and VIP customer tiers, validity-window pricing rules, strategy-based price calculation, tier assignment, customer catalog pricing, and order price snapshots. Admins manage pricing rules; SalesEmployees/Admins can change customer tiers. Historical OrderItem prices remain unchanged after pricing rules change. Documentation: `docs/pricing/customer-specific-pricing.md`.

## Epic 5 — Orders

Implemented order creation and status workflow. Customer orders validate active customers/products and inventory, snapshot product prices, merge duplicate items, reserve stock, and persist in one EF Core unit of work. Orders support customer history and ownership-protected reads; staff can confirm, reject, process, and complete orders. Rejection releases reservations and completion confirms stock. Documentation: `docs/orders/order-management.md`.

## Epic 3 — Customer Product Catalog

Added customer-facing catalog queries and `ProductCatalogController`. Customers can search active products by name or SKU with database pagination and receive current price plus available quantity. A one-to-one `Inventory` record is created at product creation with quantity zero. Inventory management now supports add, reserve, release, confirm, and adjustment operations with available quantity calculated as quantity minus reserved quantity. Verification: catalog integration tests cover authorization, search, pagination, price, availability, and inactive-product hiding.

## Epic 3 — Product Catalog

Product management is implemented across Domain, Application, Infrastructure, API, and tests. Products support protected fields, normalized unique SKUs, on-demand categories, activation/deactivation, management CRUD, paginated search, and customer-visible active-only reads. Migration `AddProductCatalog` adds product/category tables and unique indexes. Verification: 50 unit tests and 24 integration tests pass.

## Epic 2 — Customer Management

Customer management is implemented across the Domain, Application, Infrastructure, API, unit-test, and integration-test layers.

Completed capabilities:

- Customer aggregate with protected state, timestamps, activation, deactivation, and detail updates.
- CQRS commands for creation, update, activation, and deactivation.
- CQRS queries for ID, current-user profile, and paged customer listing.
- Repository and unit-of-work persistence abstractions.
- Sales employee/admin authorization for management operations.
- Customer ownership checks for private profile access.
- Validation for commands and paged listing parameters.
- Search, active-state filtering, database pagination, DTO projection, and no-tracking reads.

Recent hardening:

- Customer search uses `EF.Functions.Like` rather than applying `ToLower()` to database columns, preserving provider-side query translation and avoiding unnecessary column transformations.
- Customer list ordering now uses creation time plus ID as a deterministic tie-breaker for stable pagination.
- MediatR is now used for customer CQRS requests: commands and queries implement MediatR request interfaces, handlers implement `IRequestHandler`, and controllers dispatch through `ISender`.

Verification: unit and integration test suites pass locally (46 unit tests and 20 integration tests).

Remaining Epic 2 consideration:

- Customer deactivation now prevents the associated customer from obtaining new login tokens. Existing JWTs remain valid until normal expiration; token revocation is outside the Epic 2 scope.
