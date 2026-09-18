# ADR-008: Testing Strategy

## Context

The project needs regression safety across domain rules, API behavior, and
failure modes without slowing development with brittle or infrastructure-
heavy tests.

## Decision

Two layers: xUnit + FluentAssertions (+ NSubstitute) unit tests for domain
rules, validators, handler behavior, and model metadata; `WebApplicationFactory`
+ `HttpClient` integration tests against SQLite (in-memory default,
file-based factory for parallel writes) covering endpoints, auth/authz,
workflows, rollback, and sync persistence. External HTTP is stubbed at the
handler level. Coverlet measures coverage to *find* untested business
paths — there is no percentage gate and no meaningless tests.
Details: `docs/testing/test-strategy.md`.

## Alternatives Considered

- **Integration-only**: realistic but slow and coarse; domain edge cases
  (transitions, validity windows, discount math) are cheaper as unit tests.
- **Unit-only with mocked DbContext**: fast but never proves SQL
  translation, migrations, auth pipelines, or HTTP contracts — the bugs
  that actually ship.
- **Testcontainers SQL Server**: closest to production, but heavyweight per
  run; SQLite plus provider-aware tests (query plans, file-based
  concurrency factory) proved sufficient.

## Consequences

### Positive

- Fast suite (~30 s) that still exercises real HTTP, real EF translation,
  and real failure paths; currently 180+ tests green.
- Provider differences are handled explicitly rather than discovered late.

### Negative

- SQLite is not SQL Server: plans and locking semantics need the targeted
  verification this strategy prescribes; a Testcontainers job remains a
  possible future upgrade (see `docs/roadmap.md`).
