# Customer Product Catalog

Authenticated customers browse the active product catalog through:

- `GET /api/catalog/products`
- `GET /api/catalog/products/{id}`

The catalog supports database-level pagination (`pageNumber`, `pageSize`) and case-insensitive search by product name or SKU (`searchTerm`). Inactive products are excluded from lists and return `404` when requested by ID.

Catalog responses include product details, SKU, category, current customer price (currently sourced from `Product.Price`), and available quantity (sourced from the one-to-one `Inventory` record as `Quantity - ReservedQuantity`). New products start with zero available quantity.
