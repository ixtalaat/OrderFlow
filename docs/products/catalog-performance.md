# Catalog Performance (US-13)

## Baseline (2026-09-17)

Measured against 2000 seeded products (each with inventory) + 200 pricing
rules, file-based SQLite, page size 20, averages over 20 runs:

```text
Repository page query:  ~2.3 ms
Full handler (customer + count + page + pricing): ~4.6 ms
```

Generated SQL for `GET /api/catalog/products` is a single `SELECT` with an
`INNER JOIN` to categories, a `LEFT JOIN` to inventory, server-side
`Quantity - ReservedQuantity`, `ORDER BY Name, Id`, and `LIMIT/OFFSET`.
Pricing rules load in one batched `IN` query per page. No N+1 was found.

### Bottleneck identified

Two issues, both confirmed by inspection and query plans:

1. No index supported the catalog list/order. The default browse
   (`WHERE IsActive ORDER BY Name, Id`) sorted without index support.
2. The pricing-rule index `(ProductId, Tier)` did not cover the validity
   filter (`ValidFromUtc <= now AND (ValidToUtc IS NULL OR now < ValidToUtc)`)
   and ordering used by every catalog read.

Evaluated and deliberately left alone:

- `EF.Functions.Like("%term%")` searches are non-SARGable by construction;
  a b-tree index cannot serve leading-wildcard matches (full-text search is
  out of scope).
- The controller and the handler each load the customer once by primary key
  (sub-millisecond seeks); collapsing them would churn two query contracts
  for no measurable gain.
- Read paths already use `AsNoTracking` with DTO projections and
  database-level pagination.

## Optimization

Migration `AddCatalogPerformanceIndexes`:

- `Products(IsActive, Name)` — serves the default browse filter and ordering.
- `PricingRules(ProductId, Tier, ValidFromUtc)` — replaces `(ProductId, Tier)`;
  seeks the product/tier predicate and supports the validity range + ordering.

Query plans after the change:

```text
No-search browse: SCAN p USING COVERING INDEX IX_Products_IsActive_Name
Search ("%term%"): SCAN p   (expected: leading wildcard is non-SARGable)
```

After (same setup): repository ~2.6 ms, handler ~3.7 ms. The local numbers
are flat because 2000 rows resolve in single-digit milliseconds on any plan;
the win is at production scale, where the covering index removes the sort
and table lookups behind every browse page and count. Guarded by
`CatalogIndexTests`, which asserts both indexes exist in the EF model.

## Caching evaluation — not implemented, by measurement

Redis (or any cache) was evaluated and rejected for now:

- The full catalog handler resolves in ~4 ms; there is no latency problem
  for a cache to solve.
- `AvailableQuantity` changes on nearly every order, so cached pages would
  either churn constantly or display stale stock next to the Epic 10
  no-oversell guarantee. Prices vary per customer tier and validity window,
  exploding the key space (`tier x search x page x pageSize`).
- A cache would add invalidation surface across product, pricing-rule, and
  inventory writes plus a new infrastructure dependency, with no measured
  benefit — against the project's no-over-engineering rule.

Revisit when any of these is true: catalog p95 exceeds 200 ms on production
data, catalog reads saturate the database, or repeated identical page reads
dominate traffic. The natural first step then is short-TTL caching of the
no-search browse pages only (the path the new covering index serves), with
write-through invalidation on product and pricing-rule changes and
graceful fall-through to the database on cache failure.
