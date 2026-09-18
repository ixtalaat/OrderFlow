# Customer-Specific Pricing

Customers have one of three pricing tiers: `Regular`, `Wholesale`, or `Vip`. New customers default to `Regular`. Admins can change a customer's tier through `PATCH /api/customers/{id}/tier` with `{ "tier": "Wholesale" }` (the legacy query-string form remains accepted). Catalog pages batch-load active rules for the page to avoid one pricing query per product.

Pricing rules are product-specific percentage discounts with validity windows. Create and update operations perform overlap validation inside a serializable transaction to prevent concurrent overlapping writes. Only Admin users manage rules:

- `POST /api/pricing-rules`
- `PUT /api/pricing-rules/{id}`
- `DELETE /api/pricing-rules/{id}`

A valid rule is active when `ValidFromUtc <= now` and `ValidToUtc` is null or later than now. Overlapping rules for the same product and tier are rejected.

Regular, Wholesale, and VIP strategies calculate the final price using the active rule; without a rule, the product base price is used. Prices are rounded to two decimal places.

The customer catalog and order creation use the same pricing service. The calculated price is copied to `OrderItem.UnitPrice`, so later pricing-rule changes never alter historical orders.

## Coupons

Admin-managed discount codes (`POST/PUT/DELETE /api/coupons`) with validity
windows, minimum order totals, optional redemption caps, and activation
state. At order creation the coupon applies to the post-tier subtotal and
the code plus discount amount are snapshotted on the order; redemptions are
counted atomically in the same save, guarded by an optimistic-concurrency
token so capped coupons cannot over-redeem under parallel orders.
