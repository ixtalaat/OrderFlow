# ADR-004: Inventory Concurrency Strategy

Status: Accepted (2026-09-17)

## Context

`CreateOrderCommandHandler` used a read-check-reserve sequence: load the
`Inventory` row, compare the requested quantity against `AvailableQuantity`,
call `ReserveStock`, then save. Two requests arriving at the same time with
`Stock = 5` could both read `AvailableQuantity = 5` before either saved, so
both reservations succeeded and 10 units were sold from 5 units of stock
(overselling).

The same read-modify-write race existed in the standalone inventory commands
(`ReserveStock`, `AddStock`, `AdjustStock`, `ReleaseStock`, `ConfirmStock`)
and in the order `RejectOrder` / `CompleteOrder` handlers that release or
confirm reservations.

## Considered Options

1. **Optimistic concurrency with a `Version` token.** Every inventory mutation
   increments `Version`, which is mapped as an EF Core concurrency token. The
   `UPDATE` only succeeds when the row still carries the version that was
   read; the loser gets `DbUpdateConcurrencyException` and returns
   `409 Conflict`. No locks are held while the request runs.
2. **Pessimistic locking / serializable transactions.** Serializing writers
   (e.g. `SERIALIZABLE` isolation or `SELECT ... FOR UPDATE`) removes the race
   but holds locks for the whole reservation, reduces throughput under load,
   risks deadlocks, and behaves differently on the SQLite test provider than
   on SQL Server.
3. **Separate stock-counting infrastructure** (dedicated read store, message
   queue, event sourcing). Rejected: no architectural requirement justifies
   distributed CQRS for a single-database application (see the CQRS rules).

## Decision

Option 1: optimistic concurrency.

- `Inventory.Version` is the concurrency token (configured in
  `InventoryConfiguration`, already migrated). It is the only token needed:
  overselling is an inventory-row problem, so no token was added to `Order`.
- Reservation stays inside one unit-of-work save. `CreateOrder` persists the
  order, the inventory reservations, and the notification outbox row in a
  single `SaveChangesAsync`, which is atomic: order and reservation either
  commit together or roll back together.
- `UnitOfWork` (Infrastructure, the only layer allowed to reference EF Core)
  translates `DbUpdateConcurrencyException` into
  `ConcurrencyConflictException`, an Application-level exception. Stock
  handlers catch it and return typed `409 Conflict` results
  (`OrderErrors.ConcurrencyConflict`, `InventoryErrors.ConcurrencyConflict`)
  instead of leaking an unhandled exception. `GlobalExceptionHandler` still
  maps any escaping `DbUpdateConcurrencyException` to 409 as a backstop.
- API contract: the loser of a stock race receives `409 Conflict` with a
  retryable error code. Clients retry (re-reading stock) rather than the
  server blocking writers.

## Consequences

- Concurrent orders for the last units cannot oversell: exactly one request
  commits; the other gets 409 (`InsufficientStock` when it arrives after the
  commit, `ConcurrencyConflict` when its write collides).
- Throughput is unaffected in the common low-contention case; conflicting
  clients retry instead of waiting on locks.
- Tests: `Overlapping_Inventory_Reservations_Should_Conflict_On_Save` proves
  the token rejects a stale write; `Concurrent_Orders_For_Last_Stock_Should_Not_Oversell`
  fires two real `POST /api/orders` (5 units each against stock 5) and asserts
  one `201 Created`, one `409 Conflict`, and final `Reserved = 5`,
  `Available = 0`.
- Test-provider note: the shared single-connection in-memory SQLite factory
  cannot run parallel write requests (one connection handle cannot host
  overlapping transactions; the loser failed with `SQL logic error` / 500
  instead of a token conflict). Concurrency tests therefore use
  `ConcurrencyWebApplicationFactory`, a file-based SQLite factory where each
  request gets its own blocking connection, matching SQL Server row-lock
  behavior. Production code needed no provider-specific handling.
