# Project Progress

## Production hardening (pre-MonsterASP)

SQL Server transient retries (`EnableRetryOnFailure`), Data Protection keys persisted under `App_Data/keys`, rolling file logs in Production (`App_Data/logs/`), and optional `Cors:AllowedOrigins` support with preflight tests. Deployment checklist in `docs/cicd/monsterasp-deployment.md`, including the idle-timeout caveat for background jobs (keep-alive via `/health/live`).

## Epic 16 — Documentation & Portfolio (US-18)

Added overview, architecture (with diagram), database design (with ERD), and authentication docs; renumbered the concurrency ADR into a new `docs/architecture/adr/001-008` set covering architecture, SQL Server, auth, concurrency, background processing, accounting, caching, and testing; added roadmap; verified the Postman collection against current routes; captured a Scalar API screenshot; rewrote the README portfolio-ready. Also added `restart: unless-stopped` to the API compose service after observing a SQL-recovery startup race, documented in docker-setup troubleshooting.

## Epic 15 — CI/CD (US-17)

`.github/workflows/ci.yml`: build + unit tests with coverage, integration tests (TRX reports on PRs), Docker build of both images with GHCR push on master (`:sha` + `:latest`), and a master-only compose deploy smoke test (readiness + admin login, ephemeral secrets, full teardown). Failed tests block everything downstream. See `docs/cicd/deployment.md`.

## Epic 14 — Docker (US-16)

Multi-stage Dockerfiles for the API and fake accounting API, `docker-compose.yml` (SQL Server 2022 with healthcheck + persistent `mssql-data` volume, fake accounting, API with env-based config), `.env.example`, and `docs/docker/docker-setup.md`. Verified live: images build, clean startup with migrations + admin seed, `/health/ready` Healthy, admin login, product/stock/order/confirm flow with real accounting invoice, and data surviving compose down/up. No Redis (not in stack).

## Epic 13 — Testing (US-15)

Coverage audit (Coverlet) drove the work: added validator tests (order/product/pricing/paged queries), pricing service and validity-window tests, domain transition/exception tests, `AccountingService` retry/timeout/idempotency unit tests, accounting-sync persistence tests, order rollback and full-workflow integration tests. Removed one duplicated validator test. Strategy recorded in `docs/testing/test-strategy.md`.

## Epic 12 — Observability (US-14)

Requests are traceable via `X-Correlation-ID` (accepted or generated, echoed on responses including errors, enriched into every Serilog event and problem-details `traceId`). Order lifecycle and inventory handlers log ID-only business operations; accounting/outbox/email integration outcomes were already logged. Health split into `/health` + `/health/ready` (EF Core database check) and check-free `/health/live`; no Redis check (no Redis in stack). Exception-to-status mappings pinned by unit tests; health and correlation covered by integration tests. See `docs/observability/observability.md`.

## Epic 11 — Performance & Caching (US-13)

Catalog baseline on 2000 seeded products: single SELECT with category/inventory joins, server-side availability, batched pricing — no N+1. Added migration `AddCatalogPerformanceIndexes` (`Products(IsActive, Name)` covering index, `PricingRules(ProductId, Tier, ValidFromUtc)`); query plans confirm covering-index browse, leading-wildcard search stays a scan (accepted). Caching evaluated and rejected by measurement (~4 ms handler, volatile stock data, tier key explosion); revisit criteria documented. See `docs/products/catalog-performance.md`.

## Epic 10 — Concurrency & Reliability (US-12)

Inventory reservations are concurrency-safe: `Inventory.Version` (EF Core concurrency token) plus a single atomic unit-of-work save per order means two simultaneous orders for the last stock cannot oversell — one commits, the other gets `409 Conflict`. `UnitOfWork` translates `DbUpdateConcurrencyException` to `ConcurrencyConflictException` so Application handlers return typed 409 results without referencing EF Core; `OrderErrors.ConcurrencyConflict` was added. Verified by parallel `POST /api/orders` integration test (one 201, one 409, reserved 5/available 0), a deterministic stale-write token test, and handler unit tests. Concurrency tests use a file-based SQLite factory because the shared single-connection in-memory factory cannot run parallel writes. See `docs/architecture/adr/004-inventory-concurrency.md`.

## Epic 9 — Accounting integration

The accounting status and external invoice reference are included in order read models for observability.

Added a simulated external accounting API, typed `HttpClient` integration, bounded transient retries, timeout configuration, idempotency keys, and observable accounting synchronization state on orders. Confirmed orders create accounting outbox messages; successful external invoice IDs and failures are persisted. See `docs/accounting/accounting-integration.md`.

## Authentication hardening

Auth now uses Identity lockout-aware password validation, validates JWT configuration at startup, reports the configured token expiration, compensates for registration role/profile failures, and applies IP-based rate limiting to auth endpoints. Email confirmation remains an explicit future decision because registration currently issues tokens immediately. See `docs/authentication/auth-hardening.md`.

## Reliability hardening — Outbox and startup safety

Added a transactional outbox for order notifications and accounting synchronization. Order handlers add outbox rows before the same unit-of-work save; `OutboxDispatcher` publishes pending rows to Hangfire with at-least-once semantics. Production startup now applies EF migrations, while Testing retains SQLite `EnsureCreatedAsync`. Public registration creates a Customer profile, catalog pricing loads active rules in one batch, product creation uses the transactional unit of work, and the customer tier API accepts a JSON DTO (with legacy query compatibility). Email is validated when enabled and cannot be disabled in Production. Migration: `AddOutbox`. See `docs/background-processing.md`.

## Epic 8 — Order Notifications

Added MailKit email delivery, order notification templates, and notification services. Hangfire notification jobs are queued after every relevant order status change, use three retries, log failures, and remain retryable from the protected dashboard. Email is disabled in test environments through configuration.

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
