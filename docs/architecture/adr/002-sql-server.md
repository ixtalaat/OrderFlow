# ADR-002: SQL Server

## Context

OrderFlow needs a relational store for orders, inventory, and pricing with
transactional guarantees (order + reservations + outbox must commit
atomically), plus a zero-friction provider for automated tests.

## Decision

SQL Server in production and local Docker (`mcr.microsoft.com/mssql/server`),
via EF Core Code First with migrations applied at startup. Tests use SQLite
(file-based for parallel-write scenarios, otherwise shared in-memory) with
`EnsureCreatedAsync`, so no test run needs a server.

## Alternatives Considered

- **PostgreSQL**: fully capable and cheaper to host, but SQL Server matches
  the portfolio's Microsoft stack (ASP.NET Core, Identity, Hangfire SQL
  storage) with the least integration risk.
- **SQLite everywhere**: removes the server but gives up production-grade
  concurrency semantics (blocking row locks) that the inventory strategy
  relies on — proven by the test-harness differences in ADR-004.

## Consequences

### Positive

- ACID transactions, Hangfire SQL storage, and mature EF Core support.
- One-line local parity through Docker Compose with a persistent volume.

### Negative

- Heavier local footprint (~500 MB image) than SQLite/Postgres-alpine.
- Test provider differs from production; provider-sensitive behavior
  (concurrency, query plans) is verified against the production semantics,
  not just SQLite.
