# Inventory Management

Inventory is a one-to-one aggregate for each product. It stores `Quantity`, `ReservedQuantity`, and an application-managed `Version`. `AvailableQuantity` is calculated as `Quantity - ReservedQuantity`.

## Operations

- `AddStock`: increases total quantity.
- `ReserveStock`: increases reserved quantity when enough available stock exists.
- `ReleaseStock`: decreases reserved quantity.
- `ConfirmStock`: decreases both quantity and reserved quantity.
- `AdjustStock`: applies a positive or negative adjustment, but never below reserved stock.

All quantities used by stock operations must be positive. Inventory cannot become negative.

## API

All endpoints require the `Admin` or `SalesEmployee` role:

- `GET /api/inventory/{productId}`
- `POST /api/inventory/{productId}/add-stock`
- `POST /api/inventory/{productId}/reserve`
- `POST /api/inventory/{productId}/release`
- `POST /api/inventory/{productId}/confirm`
- `PATCH /api/inventory/{productId}/adjust`

## Concurrency

`Version` is configured as an EF Core concurrency token. Every successful domain mutation increments it. If another request changes the same inventory row before save, the unit of work translates the resulting `DbUpdateConcurrencyException` into `ConcurrencyConflictException` (the Application layer never references EF Core directly) and the handler returns a typed `409 Conflict` result (`InventoryErrors.ConcurrencyConflict`); the global exception handler maps any escaping concurrency exception to `409 Conflict` as a backstop. This application-managed token works with both SQL Server and the SQLite integration-test provider.

Reservations are transactional: order creation persists the order, the inventory reservations, and the notification outbox row in a single unit-of-work save, so the order and its reservations commit or roll back together. Concurrent orders for the last units cannot oversell — one request commits and the other receives `409 Conflict` (`InsufficientStock` when it arrives after the commit, `ConcurrencyConflict` when its write collides). Full rationale: `docs/architecture/adr-001-inventory-concurrency.md`.
