# Customer Management

Public customer registration now creates both the Identity user and its Customer profile. The tier endpoint accepts a JSON body, for example `{ "tier": "Wholesale" }`, and validates the tier name. Documentation

## Overview
Customer Management (Epic 2, US-02) enables sales employees and administrators to manage customer profiles, and allows customers to securely access their own profile data.

## Architecture
The Customer Management module follows **Clean Architecture** and **CQRS**:
- **Domain Layer (`OrderFlow.Domain`)**: Encapsulates customer aggregate rules, state protection, and business invariants in `Customer.cs`.
- **Application Layer (`OrderFlow.Application`)**: Defines CQRS commands/queries, handlers, DTOs, FluentValidation rules, repository interface `ICustomerRepository`, and unit of work interface `IUnitOfWork`.
- **Infrastructure Layer (`OrderFlow.Infrastructure`)**: Implements persistence with EF Core, `CustomerConfiguration`, `CustomerRepository`, `UnitOfWork`, and Identity management through `IIdentityService`.
- **API Layer (`OrderFlow.API`)**: Exposes REST endpoints via thin `CustomersController` dispatching commands and queries.

---

## Domain Model
```csharp
public class Customer
{
    public int Id { get; private set; }
    public string UserId { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Address { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public static Customer Create(string userId, string phoneNumber, string address);
    public void UpdateDetails(string phoneNumber, string address);
    public void Activate();
    public void Deactivate();
}
```

---

## API Endpoints & Authorization Matrix

| Method | Endpoint | Allowed Roles | Description |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/customers` | `Admin`, `SalesEmployee` | Create a new customer user and profile |
| `GET` | `/api/customers` | `Admin`, `SalesEmployee` | List customers with search and pagination |
| `GET` | `/api/customers/me` | `Customer` | Retrieve current authenticated customer profile |
| `GET` | `/api/customers/{id}` | `Admin`, `SalesEmployee`, `Customer` (Owner only) | Get customer by ID |
| `PUT` | `/api/customers/{id}` | `Admin`, `SalesEmployee` | Update customer details |
| `PATCH` | `/api/customers/{id}/activate` | `Admin`, `SalesEmployee` | Activate customer |
| `PATCH` | `/api/customers/{id}/deactivate` | `Admin`, `SalesEmployee` | Deactivate customer |

---

## CQRS Operations

### Commands
- `CreateCustomerCommand(string FullName, string Email, string Password, string PhoneNumber, string Address)`
- `UpdateCustomerCommand(int Id, string FullName, string PhoneNumber, string Address)`
- `ActivateCustomerCommand(int Id)`
- `DeactivateCustomerCommand(int Id)`

### Queries
- `GetCustomerByIdQuery(int CustomerId)`
- `GetCustomerByUserIdQuery(string UserId)`
- `GetCustomersQuery(CustomerQueryParams QueryParams)`

## Query and Pagination Behavior
- Customer list reads use `AsNoTracking()` and project directly to `CustomerResponse` DTOs.
- Search covers full name, email, phone number, and address using `EF.Functions.Like` so filtering remains database-translatable without applying transformations to stored columns.
- `IsActive` optionally filters active or inactive profiles.
- Pagination is applied in the database with `Skip` and `Take`.
- Results are ordered by newest creation time, then customer ID as a deterministic tie-breaker.

## Epic 2 Status
The customer-management acceptance scope is implemented, including CRUD-style management, activation/deactivation, pagination, search, validation, role authorization, ownership protection, unit tests, and integration tests. Customer deactivation affects the customer profile state and prevents the associated customer from obtaining new login tokens. Existing JWTs remain governed by their normal expiration and validation rules.
