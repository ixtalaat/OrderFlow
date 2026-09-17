# Product Management

Epic 3 adds a product catalog managed by administrators and sales employees and browsable by authenticated customers.

## Model and rules

- Products have a required name, description, non-negative price, category, and SKU.
- SKU values are trimmed, normalized to uppercase, validated as `[A-Za-z0-9][A-Za-z0-9-]{2,49}`, and enforced unique by the database.
- Products and categories have protected domain state. Products start active and can be activated or deactivated.
- Categories are created on demand when a product references a new category name.

## API

| Method | Endpoint | Authorization | Purpose |
|---|---|---|---|
| POST | `/api/products` | Admin, SalesEmployee | Create a product |
| GET | `/api/products` | Admin, SalesEmployee, Customer | List products |
| GET | `/api/products/{id}` | Admin, SalesEmployee, Customer | Retrieve a product |
| PUT | `/api/products/{id}` | Admin, SalesEmployee | Update a product |
| PATCH | `/api/products/{id}/activate` | Admin, SalesEmployee | Activate a product |
| PATCH | `/api/products/{id}/deactivate` | Admin, SalesEmployee | Deactivate a product |

Customer reads through `/api/products` return active products. Management reads can include inactive products and filter with `isActive`, `searchTerm`, `pageNumber`, and `pageSize`. The customer-facing catalog is available at `/api/catalog/products`; it supports active-only listing, name/SKU search, pagination, current product price, and available quantity.

## Persistence

`ProductConfiguration` and `CategoryConfiguration` define required lengths, price precision, the restricted category relationship, and unique indexes for SKU and category name. Migration `AddProductCatalog` creates the catalog tables and indexes.
