# ADR-006: Accounting Integration Strategy

## Context

Confirmed orders must be invoiced in an external accounting system that is
slower and less reliable than our database, without blocking order
processing or double-billing on retries.

## Decision

Typed `HttpClient` integration behind `IAccountingService`, invoked from
the outbox-driven sync job (ADR-005): bounded transient retries with
backoff, timeout configuration, a stable idempotency key
(`Order:{id}:Invoice`) sent on every attempt, and persisted sync state on
the order (`Pending/Succeeded/Failed`, attempts, last error, invoice id).
A `FakeAccountingApi` stub (including fail/timeout modes) supports local
and manual end-to-end runs; automated tests stub the HTTP handler.
Details: `docs/accounting/accounting-integration.md`.

## Alternatives Considered

- **Synchronous invoicing during order confirmation**: couples checkout to
  external availability; a timeout would fail an otherwise valid order.
- **Fire-and-forget without idempotency**: retries after a timeout could
  create duplicate invoices — unacceptable for money movement.
- **Full message-broker integration**: same objection as ADR-005; the
  outbox already provides durability.

## Consequences

### Positive

- Order flow never waits on accounting; failures are visible on the order
  and retried safely without duplicates.
- The fake API makes the integration demoable without a real vendor.

### Negative

- Invoices lag order confirmation (eventual consistency); clients must read
  the sync status fields rather than assume immediate invoicing.
