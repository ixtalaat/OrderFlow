# ADR-012: OpenTelemetry Observability

## Context

Observability was Serilog + correlation IDs + health probes. The roadmap
asked for traces/metrics depth.

## Decision

OpenTelemetry in `OrderFlow.API` (off in `Testing`): ASP.NET Core +
HttpClient + EF Core + runtime instrumentation, `OrderFlow.Api`
`ActivitySource`/`Meter` (`orderflow.orders.created`,
`orderflow.accounting.syncs`), OTLP export when `Telemetry:OtlpEndpoint`
is set. Compose adds an OTLP collector (Prometheus `:8889`), Prometheus,
and Grafana. No API `/metrics` endpoint — Prometheus scrapes the
collector.

## Consequences

- Positive: standard traces/metrics without vendor lock-in; dashboards via
  Grafana; zero overhead in tests.
- Negative: three more compose services for local full-stack observability;
  OTel packages carry advisories (tracked via Dependabot/renovate).
