# Order Management

Orders are created by authenticated customers with status `Submitted` and reserve inventory atomically with order persistence. Product prices are snapshotted into order items.

## Idempotency

`POST /api/orders` accepts an optional `Idempotency-Key` header (per-user,
24-hour TTL). Repeating a request with the same key and payload replays the
stored `201` without reserving again; the same key with a different payload
returns `422`. Rationale: `docs/architecture/adr/010-order-idempotency.md`.

## Cancellation and coupons

Customers cancel their own submitted orders with
`PATCH /api/orders/{id}/cancel` (releases reservations; other statuses and
other customers' orders are rejected). Orders accept an optional coupon
code: tier pricing applies first, then the coupon discount on the subtotal,
with the code and discount amount snapshotted on the order.

## Status workflow

```text
Draft → Submitted → Confirmed → Processing → Completed
                 ├→ Rejected (staff)
                 └→ Cancelled (customer)
```

The create workflow constructs and submits the order in one operation. Rejection releases each item's reservation; completion confirms each reservation and removes stock. Invalid transitions are rejected.

## API

Customers use `POST /api/orders` and can access their own order history with `GET /api/orders` and individual orders with `GET /api/orders/{id}`. Admins and sales employees can view orders and transition them with `/confirm`, `/reject`, `/processing`, and `/complete` PATCH endpoints.

Order creation validates an active customer, active products, positive quantities, and available inventory. Duplicate products are merged. A single scoped EF Core save persists the order, inventory reservations, and notification outbox row; failures roll back all changes. Inventory concurrency conflicts return `409 Conflict` with `Order.ConcurrencyConflict` (colliding write) or `Order.InsufficientStock` (stock already taken). Decision record: `docs/architecture/adr/004-inventory-concurrency.md`.
