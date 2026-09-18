# OrderFlow — Project Overview

## Business problem

Small retailers taking orders by phone, spreadsheet, or paper lose track of
stock, misprice customer segments, and cannot tell customers where an order
stands. OrderFlow is an order management backend that keeps products,
inventory, customer-specific pricing, orders, notifications, and accounting
in one consistent system.

## Main features

- **Authentication & roles** — ASP.NET Core Identity with `Admin`,
  `SalesEmployee`, and `Customer` roles, JWT access tokens, lockout and rate
  limiting (`docs/authentication/`).
- **Customer management** — staff-owned customer profiles with activation
  state and pricing tiers (Regular, Wholesale, VIP).
- **Product catalog & inventory** — products with categories and SKUs,
  per-product stock with reservations, and a customer-facing catalog with
  search and pagination.
- **Customer-specific pricing** — time-bounded pricing rules per tier;
  order prices are snapshotted so history never changes.
- **Orders** — customers place orders that atomically reserve stock;
  staff drive `Submitted → Confirmed → Processing → Completed` (or
  `Rejected`, which releases reservations). Overselling is impossible by
  design (`docs/architecture/adr-004-inventory-concurrency.md`).
- **Notifications & accounting** — transactional outbox with Hangfire
  delivery: email on every status change, external invoicing with retries
  and idempotency keys.
- **Observability, performance, quality** — correlation IDs, structured
  logs, health probes; covering indexes with a measured no-cache decision;
  180+ automated tests; Docker Compose environment; CI with test gates and
  GHCR deploys.

## Tech stack

ASP.NET Core 10, C#, Entity Framework Core (SQL Server; SQLite in tests),
Identity + JWT, FluentValidation, MediatR, Hangfire, MailKit, Serilog,
xUnit + FluentAssertions + NSubstitute, Coverlet, Docker.

## Repository map

```text
src/OrderFlow.Api/            controllers, middleware, DI, health
src/OrderFlow.Application/    CQRS commands/queries/handlers, DTOs, validators, repo interfaces
src/OrderFlow.Domain/         entities, enums, domain rules
src/OrderFlow.Infrastructure/ EF Core, repositories, unit of work, Identity, Hangfire, email, accounting client
src/OrderFlow.FakeAccountingApi/  stub external invoicing service (dev/manual E2E)
tests/OrderFlow.Tests/        unit tests
tests/OrderFlow.IntegrationTests/  API + database tests
docs/                         feature and cross-cutting documentation
postman/                      Postman collection + environment
```

## Start here

- Run it: `README.md` (dotnet or Docker Compose).
- Understand it: `docs/architecture/architecture.md`, then
  `docs/database/database-design.md`.
- Evaluate it: `docs/testing/test-strategy.md` and the ADRs in
  `docs/architecture/adr/`.
