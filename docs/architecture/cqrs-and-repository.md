# CQRS, Repository & Unit of Work in OrderFlow

## Overview
OrderFlow implements the **CQRS (Command Query Responsibility Segregation)** pattern combined with the **Repository Pattern** and **Unit of Work Pattern**.

## Structure & Responsibilities
### MediatR Dispatching
- The Application project references `MediatR` and registers handlers from its assembly in `AddApplication()`.
- Commands and queries implement `IRequest<Result>` or `IRequest<Result<T>>`.
- Controllers depend on `ISender` and dispatch requests with `sender.Send(...)`; they do not resolve concrete handlers directly.

### Commands & Handlers
- Commands mutate state.
- Handlers coordinate domain logic, invoke repository write methods, and commit transactions using `IUnitOfWork.SaveChangesAsync()`.
- Return `Result` or `Result<T>`.

### Queries & Handlers
- Queries read data without mutating state.
- Handlers leverage repository query methods optimized with `AsNoTracking()` and projection directly to DTOs (`CustomerResponse`).

### Persistence Boundary
- `ICustomerRepository` abstracts EF Core query operations from the Application layer.
- `IUnitOfWork` represents the transactional boundary.
- `IIdentityService` abstracts user account operations from EF Core / Identity details.
