# Order Management

Orders are created by authenticated customers with status `Submitted` and reserve inventory atomically with order persistence. Product prices are snapshotted into order items.

## Status workflow

```text
Draft → Submitted → Confirmed → Processing → Completed
                 └→ Rejected
```

The create workflow constructs and submits the order in one operation. Rejection releases each item's reservation; completion confirms each reservation and removes stock. Invalid transitions are rejected.

## API

Customers use `POST /api/orders` and can access their own order history with `GET /api/orders` and individual orders with `GET /api/orders/{id}`. Admins and sales employees can view orders and transition them with `/confirm`, `/reject`, `/processing`, and `/complete` PATCH endpoints.

Order creation validates an active customer, active products, positive quantities, and available inventory. Duplicate products are merged. A single scoped EF Core save persists the order and inventory reservations; failures roll back both changes. Inventory concurrency conflicts return `409 Conflict`.
