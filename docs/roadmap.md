# Roadmap — Future Improvements

Explicitly deferred during development; each is a scoped future epic.

- ~~Email confirmation, token revocation, refresh tokens, order
  idempotency, customer cancellation, coupons, audit trail~~ — done; see
  ADRs 009–010 and the feature docs.
- **Full-text catalog search**: evaluated 2026-09-26 and deferred — the
  stock SQL Server Linux image ships no Full-Text Engine (error 7609), so
  `LIKE` is retained; needs an FTS-enabled image or a search service past
  ~100k products (see ADR-011).
- ~~Caching revisit~~ — done 2026-09-26: triggers re-checked, no-cache
  stands (see ADR-007).
- ~~Testcontainers SQL Server job~~ — done: `sqlserver-tests` CI job uses
  self-managed Testcontainers (see `docs/architecture/adr/008-testing-strategy.md`).
- ~~Accounting provider seam~~ — done 2026-09-26: `Fake`/`GenericRest`/
  `Disabled` switch behind `IAccountingService` (see
  `docs/accounting/accounting-integration.md`).
- ~~Observability depth~~ — done 2026-09-26: OpenTelemetry traces/metrics
  via OTLP collector + Prometheus + Grafana (see
  `docs/observability/observability.md`).
- **Multi-currency and tax**: prices are single-currency, tax-exclusive.
- **Production hardening**: HTTPS termination, secret management (Key Vault),
  managed database with backups, Azure/container hosting pipeline past GHCR.
