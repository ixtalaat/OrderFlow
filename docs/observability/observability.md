# Observability (US-14)

## Logging pipeline

Serilog is the only logging pipeline, configured in `OrderFlow.API`:

- Development/Testing: human-readable Console output.
- Production (`appsettings.Production.json`): single Console sink with the
  Compact JSON formatter for log aggregation.
- Minimum levels: `Information` by default, `Warning` for `Microsoft.AspNetCore`
  and `Microsoft.EntityFrameworkCore`. All events enrich `FromLogContext`.

## Request tracing

- `CorrelationIdMiddleware` (`OrderFlow.API/Observability`) accepts an
  inbound `X-Correlation-ID` header or generates one, assigns it to
  `HttpContext.TraceIdentifier`, pushes it as the Serilog `CorrelationId`
  property (present on every log event of the request), and echoes it back
  as a response header — including on error responses.
- `UseSerilogRequestLogging` runs after the middleware, so access logs carry
  the correlation ID plus method, path, status code, and elapsed time.
- Problem-details error responses include the same value as `traceId`.

## Business-operation and integration logs

Handlers log completed operations with IDs only (`OrderId`, `CustomerId`,
`ProductId`, quantities, availability): order creation and every status
transition, plus all inventory mutations. External integration outcomes are
logged where they happen: accounting retry attempts and synchronization
failures (with `OrderId`), outbox dispatch failures, and disabled-email
notifications. Concurrency losers log a `Warning` with the customer ID.

## Errors

`GlobalExceptionHandler` logs every unhandled exception with its
`TraceIdentifier` and maps it to a status code: 404 not-found, 400
validation, 409 conflicts (including `ConcurrencyConflictException` and
`DbUpdateConcurrencyException`), 401/403 auth, 500 otherwise. Mappings are
pinned by `GlobalExceptionHandlerTests`.

## Health checks

- `GET /health` and `GET /health/ready` — readiness with the `database`
  (EF Core `DbContext`) check and per-check durations as JSON.
- `GET /health/live` — liveness with no checks (always 200 when the process
  runs). There is no Redis check because the stack uses no Redis
  (see `docs/products/catalog-performance.md`).

## OpenTelemetry (traces + metrics)

`OrderFlow.API` registers OpenTelemetry (disabled in `Testing`):

- Tracing: ASP.NET Core, HttpClient, EF Core, plus the `OrderFlow.Api`
  `ActivitySource`. OTLP export when `Telemetry:OtlpEndpoint` is set
  (`http://otel-collector:4317` in compose).
- Metrics: ASP.NET Core, HttpClient, .NET runtime, plus the `OrderFlow.Api`
  meter (`orderflow.orders.created`, `orderflow.accounting.syncs`).
- Collector (`docker/otel-collector-config.yaml`) receives OTLP and exposes
  Prometheus format on `:8889`; `docker/prometheus.yml` scrapes it and
  Grafana runs on `:3000` (see `docker-compose.yml`). No API `/metrics`
  endpoint — Prometheus scrapes the collector.

Correlation IDs remain the log-level trace key; OTel spans carry their own
trace IDs. Both are returned/logged per request.

## Sensitive information

Never logged: passwords, JWTs and signing keys, connection strings, email
credentials, request bodies. Customer emails appear only where needed to
act on a delivery failure. Config secrets (`Jwt:SecretKey`, `AdminSeed`
password) are validated at startup and never written to logs.
