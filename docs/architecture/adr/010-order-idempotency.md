# ADR-010: Order Idempotency

## Context

Client retries and double-clicks on `POST /api/orders` created duplicate
orders and double reservations — the money-affecting counterpart to the
oversell race in ADR-004.

## Decision

Optional `Idempotency-Key` header on order creation, scoped per user with
24-hour TTL. The first success stores the request hash plus the serialized
response; a repeat with the same key and payload replays the stored `201`
without touching inventory, while the same key with a different payload
returns `422`. Absent header preserves existing behavior.

## Alternatives Considered

- **Natural keys (user + cart snapshot hash)**: implicit and collision-prone
  across distinct intents; explicit keys put control with the client.
- **Database unique constraint on (user, items)**: blocks legitimate repeat
  purchases of the same items.
- **Response caching by request hash alone**: cannot distinguish intentional
  repeats from retries without the client-provided key.

## Consequences

### Positive

- Retries are safe by default for clients that opt in; no duplicate charges
  on stock from network retries.

### Negative

- TTL storage per user; clients that never send keys get no protection
  (documented in the API examples).
