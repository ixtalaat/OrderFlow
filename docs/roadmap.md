# Roadmap — Future Improvements

Explicitly deferred during development; each is a scoped future epic.

- ~~Email confirmation, token revocation, refresh tokens, order
  idempotency, customer cancellation, coupons, audit trail~~ — done; see
  ADRs 009–010 and the feature docs.
- **Full-text catalog search**: leading-wildcard `LIKE` cannot use indexes;
  evaluate SQL Server full-text or a search service past ~100k products.
- **Caching**: revisit per ADR-007 triggers (p95 > 200 ms, DB saturation).
- **Testcontainers SQL Server job**: closest-to-production CI signal on top
  of the current SQLite suites (ADR-008).
- **Real accounting vendor**: replace `FakeAccountingApi` behind the
  existing `IAccountingService` seam.
- **Multi-currency and tax**: prices are single-currency, tax-exclusive.
- **Observability depth**: OpenTelemetry traces/metrics and log aggregation
  beyond Console JSON.
- **Production hardening**: HTTPS termination, secret management (Key Vault),
  managed database with backups, Azure/container hosting pipeline past GHCR.
