# ADR-005: Background Processing Strategy

## Context

Order notifications and accounting synchronization must not run inside the
user's HTTP request (slow SMTP/external API would inflate latency and risk
partial failure), yet they must never be lost when the request succeeds.

## Decision

Transactional outbox + Hangfire. Handlers append outbox rows in the same
unit-of-work save as the business change (atomic); `OutboxDispatcher`
publishes pending rows to Hangfire with at-least-once semantics, three
automatic retries, and a role-protected dashboard. Test environments use a
no-op scheduler. Details: `docs/background-processing.md`.

## Alternatives Considered

- **Inline await in the request**: simplest, but couples response latency to
  SMTP/accounting availability and loses work on process crash.
- **Hosted-service channel/queue in memory**: survives slow dependencies
  but not restarts; still no dashboard or retry visibility.
- **External broker (RabbitMQ/Azure Service Bus)**: durable and scalable,
  but a whole new infrastructure dependency for two message types.

## Consequences

### Positive

- No lost side effects: outbox row and business change commit together.
- Retries, failure retention, and an observable dashboard out of the box.

### Negative

- At-least-once delivery: consumers must be idempotent (accounting uses
  idempotency keys; notifications are naturally repeatable).
- Hangfire shares the SQL Server database (operational coupling, acceptable
  at this scale).
