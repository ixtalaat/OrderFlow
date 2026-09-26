# ADR-011: Catalog Full-Text Search — Evaluated, Deferred

## Context

Catalog search used leading-wildcard `LIKE '%term%'`, a full scan by
construction. The roadmap asked to evaluate SQL Server full-text search.

## Evaluation (2026-09-26)

Implemented end to end on a live stack: migration
`AddProductFullTextSearch` plus `EF.Functions.FreeText` with a `LIKE`
fallback. It failed in practice for two reasons:

1. The stock SQL Server Linux image (`mcr.microsoft.com/mssql/server:2022-latest`,
   also used by the Testcontainers CI job) ships **no Full-Text Engine**:
   `CREATE FULLTEXT INDEX` fails with error 7609 ("Full-Text Search is
   not installed, or a full-text component cannot be loaded"), which
   crash-looped the API at startup until reverted. (`CREATE FULLTEXT
   CATALOG` alone succeeds — metadata only — so the failure surfaces
   only at index creation.)
2. A hand-written migration without the `[Migration]` attribute is
   invisible to `MigrateAsync`; the Designer partial carrying the
   attribute is required.

## Decision

`LIKE` retained. The migration is kept as a guarded no-op (creates the
catalog/index only when `FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') = 1`)
so FTS-capable deployments get the index without breaking stock images;
the repository uses `LIKE` on all providers until the engine is actually
available.

## Consequences

- Positive: search works identically on every supported environment; no
  startup risk; migration history stays consistent.
- Negative: leading-wildcard search stays a scan; revisit with an
  FTS-enabled SQL image (or a search service) past ~100k products.
