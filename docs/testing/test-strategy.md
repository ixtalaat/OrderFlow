# Test Strategy (US-15)

## Layers

- **Unit** (`tests/OrderFlow.Tests`, xUnit + FluentAssertions + NSubstitute):
  domain rules and exceptions, validators (theory per rule), handler
  behavior with substituted repositories (including concurrency-conflict
  mapping), pricing calculation, model metadata (catalog indexes),
  exception-to-status mapping, Hangfire retry attributes.
- **Integration** (`tests/OrderFlow.IntegrationTests`,
  `WebApplicationFactory<Program>` + `HttpClient`): endpoints, auth/authz,
  database behavior, full order workflows, rollback atomicity, pricing
  validity windows, accounting-synchronization persistence, health and
  correlation.

## Test doubles

- SQLite in-memory (single shared connection) is the default test database;
  the app creates the schema with `EnsureCreatedAsync` in the Testing
  environment, so no migrations run in tests.
- `ConcurrencyWebApplicationFactory` (file-based SQLite, one file per
  factory, deleted on dispose) exists for tests that issue parallel write
  requests: a single shared connection cannot host overlapping transactions
  and fails with provider errors instead of real concurrency conflicts.
- SQL Server parity: setting `ORDERFLOW_TEST_DATABASE=SqlServer` runs the
  whole integration suite against real SQL Server via a Testcontainers-managed
  container (no external server needed; Ryuk reaps it afterwards). An explicit
  server can still be forced with `ORDERFLOW_TEST_SQLSERVER`. Used by the CI
  `sqlserver-tests` job with zero service configuration.
- External HTTP is stubbed, not hosted: `AccountingService` tests use a
  stub `HttpMessageHandler` (success / 5xx / 4xx / timeout / JSON null);
  the `FakeAccountingApi` project remains for manual end-to-end runs only.

## Coverage

Collected with Coverlet (`dotnet test --collect:"XPlat Code Coverage"`).
Use it to find untested business paths (this is how the missing accounting,
validator, and confirm-path tests were found), not as a percentage gate:
no meaningless tests for the sake of a number. Migrations, designer files,
and DI registrations are not meaningful coverage targets.

## Conventions

- Test names read `Subject_Should_Behavior_When_Condition`; validator tests
  assert one valid case plus a theory of invalid inputs per rule.
- One top-level test class per file; no duplicate coverage of the same rule
  in two places.
- Test data uses unique values (GUID SKUs/emails) so tests never interfere;
  integration collections run with parallelization disabled
  (`AssemblyInfo.cs`) because SQLite providers share process resources.
