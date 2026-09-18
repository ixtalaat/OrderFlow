# ADR-007: Caching Strategy

## Context

Epic 11 asked whether the customer-facing product catalog needs caching
(Redis or otherwise) to stay responsive.

## Decision

No cache. Measured handler time is ~4 ms on a 2000-product catalog with no
latency problem to solve; `AvailableQuantity` changes on nearly every order
(stale stock would undermine the Epic 10 no-oversell guarantee); per-tier
pricing explodes the key space; and invalidation would span product,
pricing-rule, and inventory writes. The catalog was instead optimized with
covering indexes (see `docs/products/catalog-performance.md`).

## Alternatives Considered

- **IMemoryCache for browse pages**: cheap but per-instance, near-useless
  with short TTLs on volatile stock data, stale with long ones.
- **Redis + IDistributedCache**: right tool for multi-instance shared
  caching, but adds infrastructure, failure modes, and invalidation
  complexity with no measured benefit.

## Consequences

### Positive

- No stale reads, no invalidation bugs, no new dependency or failure mode.

### Negative

- Every catalog read hits the database; revisit if p95 exceeds 200 ms on
  production data, reads saturate the DB, or identical page reads dominate.
  First step then: short-TTL no-search browse pages only, with
  write-through invalidation and DB fall-through.
