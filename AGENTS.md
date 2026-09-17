# OrderFlow — AI Agent Instructions

## 1. Project Overview

OrderFlow is an order management system built with ASP.NET Core and C#.

The project is developed as a portfolio-quality backend project with a focus on:

* Clean Architecture
* SOLID principles
* CQRS
* Repository Pattern
* Unit of Work Pattern
* Dependency Injection
* RESTful API design
* Authentication and Authorization
* Entity Framework Core
* Automated testing
* Database performance
* Production-oriented development practices

The project is developed incrementally through Epics and User Stories.

---

# 2. Technology Stack

## Backend

* ASP.NET Core
* C#
* Entity Framework Core
* SQL Server
* ASP.NET Core Identity
* JWT Authentication
* FluentValidation
* Serilog
* Scalar API documentation

## Architecture / Design Patterns

* Clean Architecture
* CQRS
* Repository Pattern
* Unit of Work Pattern
* Dependency Injection
* SOLID principles

## Testing

* xUnit
* FluentAssertions
* Microsoft.AspNetCore.Mvc.Testing
* HttpClient
* SQLite In-Memory Database
* Coverlet
* Coverage Analysis

---

# 3. Project Skills

The project uses the following skills/tools:

```text
aspnet-core
coverage-analysis
coverlet
entity-framework-core
microsoft-extensions
minimal-api-file-upload
optimizing-ef-core-queries
xunit
grill-me
ponytaill
postman-collection-generator
```

When a skill is relevant to the current task, use its guidance.

Use `postman-collection-generator` to create and maintain Postman collections for the OrderFlow API. When generating a collection, include the available authentication flow, environment variables, request examples, and the customer-management endpoints relevant to the current Epic.

The skill source is:
`https://github.com/patricio0312rev/skills`

Installation command:
```powershell
npx skills add https://github.com/patricio0312rev/skills --skill postman-collection-generator
```

Do not introduce functionality unrelated to the current feature simply because a skill is available.

---

# 4. Architecture

OrderFlow follows Clean Architecture with CQRS.

Expected structure:

```text
src/
├── OrderFlow.Api/
├── OrderFlow.Application/
├── OrderFlow.Domain/
└── OrderFlow.Infrastructure/

tests/
├── OrderFlow.Tests/
└── OrderFlow.IntegrationTests/

docs/
memory/
```

## Dependency Direction

```text
OrderFlow.Api
      ↓
OrderFlow.Application
      ↓
OrderFlow.Domain

OrderFlow.Infrastructure
      ↓
OrderFlow.Application
      ↓
OrderFlow.Domain
```

The Domain layer must remain independent from Infrastructure and framework-specific concerns.

---

# 5. Layer Responsibilities

## Domain

Contains:

* Entities
* Aggregates
* Domain rules
* Value Objects
* Domain exceptions
* Enums

Must not contain:

* Controllers
* HTTP concerns
* EF Core-specific persistence code
* Infrastructure implementations
* API DTOs
* CQRS handlers

Domain logic should remain independent of application and infrastructure concerns.

---

## Application

Contains:

* CQRS Commands
* CQRS Queries
* Command Handlers
* Query Handlers
* DTOs
* Validators
* Repository interfaces
* Unit of Work interfaces
* Application-specific business orchestration

The Application layer depends on Domain.

Application code should not directly depend on EF Core `DbContext` for normal feature operations.

---

## Infrastructure

Contains:

* EF Core
* DbContext
* Entity configurations
* Migrations
* Repository implementations
* Unit of Work implementation
* Identity persistence
* External service implementations

Infrastructure implements persistence and external dependency abstractions defined by Application.

---

## API

Contains:

* Controllers
* Middleware
* Authentication configuration
* Authorization configuration
* Dependency Injection registration
* API-specific concerns

Controllers should remain thin.

Controllers should dispatch commands and queries rather than implementing business logic.

---

# 6. CQRS

OrderFlow uses the **CQRS (Command Query Responsibility Segregation)** pattern.

CQRS separates operations that change state from operations that read state.

```text
Command
    ↓
Changes state

Query
    ↓
Reads state
```

The purpose is to keep write and read responsibilities clear and independently optimized.

---

# 7. Commands

Commands represent operations that change application state.

Examples:

```text
CreateCustomerCommand
UpdateCustomerCommand
ActivateCustomerCommand
DeactivateCustomerCommand
```

A command should describe an intention.

Example:

```csharp
public sealed record CreateCustomerCommand(
    string Email,
    string FullName,
    string PhoneNumber,
    string Address);
```

Commands should not contain infrastructure logic.

---

# 8. Command Handlers

Command handlers execute commands.

Example flow:

```text
POST /api/customers
        ↓
CreateCustomerCommand
        ↓
CreateCustomerCommandHandler
        ↓
CustomerRepository
        ↓
UnitOfWork
        ↓
EF Core
        ↓
Database
```

Example:

```csharp
public sealed class CreateCustomerCommandHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(
        CreateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        // Create domain entity
        // Add through repository
        // Commit through Unit of Work

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.Id;
    }
}
```

The exact implementation should follow existing project conventions.

---

# 9. Queries

Queries represent operations that read data without changing application state.

Examples:

```text
GetCustomerByIdQuery
GetCustomersQuery
SearchCustomersQuery
```

Queries should not modify domain state.

Example:

```csharp
public sealed record GetCustomerByIdQuery(int CustomerId);
```

---

# 10. Query Handlers

Query handlers are responsible for retrieving data.

Example flow:

```text
GET /api/customers/10
        ↓
GetCustomerByIdQuery
        ↓
GetCustomerByIdQueryHandler
        ↓
CustomerRepository
        ↓
EF Core
        ↓
Database
```

For read operations, prefer optimized queries and projections.

Example:

```csharp
public sealed class GetCustomerByIdQueryHandler
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByIdQueryHandler(
        ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerResponse?> Handle(
        GetCustomerByIdQuery query,
        CancellationToken cancellationToken)
    {
        return await _customerRepository.GetResponseByIdAsync(
            query.CustomerId,
            cancellationToken);
    }
}
```

---

# 11. CQRS Rules

## Commands

Commands:

* Change state.
* Use domain entities and business rules.
* Use repositories for persistence.
* Use Unit of Work to commit changes.
* Must not be used for simple read operations.

## Queries

Queries:

* Read data.
* Must not modify application state.
* Should return DTOs/read models where appropriate.
* Should use efficient database queries.
* Should use projection when appropriate.
* Should use `AsNoTracking()` for read-only queries when tracking is unnecessary.

---

# 12. CQRS Does Not Mean Two Databases

OrderFlow uses CQRS primarily as a **logical separation of reads and writes**.

Do not introduce:

* Separate read database
* Event sourcing
* Message brokers
* Distributed CQRS infrastructure

unless a future requirement explicitly justifies it.

Initially:

```text
Commands ─────┐
              ↓
          SQL Server
              ↑
Queries ──────┘
```

The project can evolve toward more advanced CQRS if there is a real architectural requirement.

---

# 13. Repository Pattern

OrderFlow uses the **Repository Pattern** to abstract persistence operations from the Application layer.

Repositories provide the persistence boundary used by commands and queries.

Example:

```csharp
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<CustomerResponse?> GetResponseByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Customer customer,
        CancellationToken cancellationToken = default);
}
```

The exact interface should be driven by actual feature requirements.

---

# 14. Repository Rules

Repositories:

* Have abstractions in Application.
* Have implementations in Infrastructure.
* Encapsulate EF Core persistence details.
* Should contain persistence-related operations.
* Should not contain controller logic.
* Should not contain application workflows.
* Should not contain unrelated business orchestration.

Do not expose `DbContext` from a repository.

Do not automatically create a generic repository for every entity.

Prefer meaningful repositories based on aggregate or feature requirements.

---

# 15. Unit of Work Pattern

OrderFlow uses the **Unit of Work Pattern** to coordinate persistence changes.

The Unit of Work represents a single logical persistence boundary.

Example:

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
```

The implementation belongs in Infrastructure.

```text
Application
    |
    +-- ICustomerRepository
    |
    +-- IUnitOfWork
             |
             ↓
Infrastructure
    |
    +-- CustomerRepository
    |
    +-- UnitOfWork
             |
             ↓
        ApplicationDbContext
             |
             ↓
          Database
```

---

# 16. CQRS + Repository + Unit of Work

These patterns work together.

## Write Flow

```text
HTTP Request
     ↓
Controller
     ↓
Command
     ↓
Command Handler
     ↓
Domain Entity / Aggregate
     ↓
Repository
     ↓
Unit of Work
     ↓
EF Core
     ↓
Database
```

## Read Flow

```text
HTTP Request
     ↓
Controller
     ↓
Query
     ↓
Query Handler
     ↓
Repository
     ↓
Optimized EF Core Query
     ↓
DTO / Read Model
     ↓
HTTP Response
```

The key distinction is:

```text
Commands → modify state → Unit of Work

Queries → read state → optimized read operation
```

---

# 17. Application Services

When CQRS is used, do not create unnecessary application services that simply duplicate command/query handlers.

Prefer:

```text
CreateCustomerCommand
CreateCustomerCommandHandler
```

over:

```text
CustomerService.Create()
```

when the operation is a clear CQRS command.

Similarly:

```text
GetCustomerByIdQuery
GetCustomerByIdQueryHandler
```

should be preferred for read operations.

Existing application services may remain if they provide a legitimate shared application responsibility.

Do not refactor existing working code unnecessarily just to force everything into CQRS.

---

# 18. EF Core Query Optimization

Repository implementations should use EF Core efficiently.

For read-only operations:

```csharp
.AsNoTracking()
```

should generally be used when tracking is unnecessary.

Prefer projection:

```csharp
.Select(x => new CustomerResponse
{
    Id = x.Id,
    Name = x.User.FullName,
    Email = x.User.Email!
})
```

Avoid loading entire entities when only a few fields are required.

Avoid N+1 queries.

Avoid unnecessary `Include()` calls.

Pagination must happen at the database level.

Example:

```csharp
var customers = await query
    .AsNoTracking()
    .Where(...)
    .OrderBy(x => x.Id)
    .Skip(...)
    .Take(...)
    .Select(...)
    .ToListAsync(cancellationToken);
```

---

# 19. Transactions

Use the Unit of Work to coordinate changes that belong to one business operation.

For example:

```text
Create Customer
      ↓
Create ApplicationUser
      ↓
Create Customer Profile
      ↓
Commit
```

If multiple changes must succeed or fail together, they should be treated as one logical unit of work.

Do not scatter `SaveChangesAsync()` calls across a single business operation.

---

# 20. Entity Guidelines

Entities should protect their state.

Prefer private setters where appropriate.

Example:

```csharp
public class Customer
{
    public int Id { get; private set; }

    public bool IsActive { get; private set; }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
```

Important business rules should be enforced by the domain model when appropriate.

---

# 21. DTO Guidelines

Do not expose EF Core entities directly from API endpoints.

Use DTOs/read models for API communication.

Examples:

```text
CreateCustomerRequest
UpdateCustomerRequest
CustomerResponse
CustomerListResponse
```

CQRS queries should generally return DTOs or read models.

Commands should receive command/request models containing only the required input.

---

# 22. Validation

Use FluentValidation for request/command validation.

Example:

```csharp
public sealed class CreateCustomerCommandValidator
    : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.PhoneNumber)
            .NotEmpty();
    }
}
```

Validation should happen before executing the command.

Business rules that depend on domain state must not be implemented only as request validation.

---

# 23. Authentication & Authorization

Current roles:

```text
Admin
SalesEmployee
Customer
```

Every new command, query, and endpoint must explicitly consider authorization.

Ask:

```text
Who can execute this command?
Who can execute this query?
Who owns this resource?
Can another customer access it?
What happens without authentication?
What happens with insufficient permissions?
```

Customers must never access another customer's private data.

Authorization should be enforced at the appropriate application/API boundary and resource ownership must be checked where necessary.

---

# 24. API Guidelines

Controllers should translate HTTP requests into commands or queries.

Example:

```csharp
[HttpPost]
public async Task<IActionResult> Create(
    CreateCustomerCommand command)
{
    var id = await _sender.Send(command);

    return CreatedAtAction(
        nameof(GetById),
        new { id },
        id);
}
```

The controller should not:

* Access `DbContext`
* Implement business rules
* Perform complex database queries
* Manage transactions
* Contain application workflows

---

# 25. Testing

The project contains:

```text
OrderFlow.Tests
OrderFlow.IntegrationTests
```

## Unit Tests

Use xUnit and FluentAssertions.

Test:

* Domain behavior
* Business rules
* Command handlers
* Query handlers
* Validators
* Important edge cases
* Authorization-related application behavior where appropriate

---

## Integration Tests

Test:

* API endpoints
* Commands through the real application pipeline
* Queries through the real application pipeline
* Authentication
* Authorization
* Database behavior
* Request/response behavior

Integration tests use:

```text
WebApplicationFactory<Program>
HttpClient
SQLite In-Memory Database
```

---

# 26. Coverage

Coverage is a supporting quality metric.

After significant features:

```text
1. Run tests
2. Collect coverage with Coverlet
3. Analyze coverage
4. Identify meaningful gaps
5. Add tests where necessary
```

Prioritize:

* Business rules
* Command handlers
* Query handlers
* Validation
* Authorization
* Error paths
* Important workflows

Do not write meaningless tests solely to increase coverage percentage.

---

# 27. Documentation

The project contains:

```text
docs/
```

Documentation represents durable project knowledge.

Possible structure:

```text
docs/
├── architecture/
├── authentication/
├── customers/
├── database/
├── api/
└── testing/
```

When a feature changes project behavior or architecture:

* Update relevant documentation.
* Add documentation where necessary.
* Document business rules.
* Document important API behavior.
* Document important architectural decisions.
* Document significant CQRS decisions.
* Document significant repository/unit-of-work decisions.

Avoid documenting trivial implementation details.

---

# 28. Agent Memory

The project contains:

```text
memory/
```

Memory stores important development context for future agents.

Possible structure:

```text
memory/
├── architecture.md
├── decisions.md
├── progress.md
├── lessons-learned.md
└── feature-notes/
```

After every completed feature, update memory with:

* What was implemented
* Important architectural decisions
* CQRS decisions
* Repository decisions
* Unit of Work decisions
* Important business rules
* Problems encountered
* Lessons learned
* Remaining decisions/TODOs
* Current project progress

Do not store unnecessary conversation details.

---

# 29. Feature Development Workflow

Every feature follows this workflow:

```text
1. Read User Story
2. Read Definition of Done
3. Inspect existing implementation
4. Read relevant docs/
5. Read relevant memory/
6. Identify domain changes
7. Identify Commands
8. Identify Queries
9. Identify repository requirements
10. Identify Unit of Work requirements
11. Implement Domain
12. Implement Commands
13. Implement Command Handlers
14. Implement Queries
15. Implement Query Handlers
16. Implement repositories
17. Implement Unit of Work changes
18. Implement API endpoints
19. Add validation
20. Add authorization
21. Add migrations if required
22. Add unit tests
23. Add integration tests
24. Run tests
25. Analyze coverage when relevant
26. Update docs/
27. Update memory/
28. Review git diff
29. Commit the feature
```

---

# 30. Feature Completion Definition

A feature is not complete until:

* Domain implementation is complete.
* Required Commands are implemented.
* Required Command Handlers are implemented.
* Required Queries are implemented.
* Required Query Handlers are implemented.
* Repository requirements are implemented.
* Unit of Work requirements are implemented.
* Validation works.
* Authorization works.
* Unit tests exist.
* Integration tests exist where appropriate.
* Tests pass.
* Coverage is reviewed when relevant.
* API documentation is updated when necessary.
* `docs/` is updated.
* `memory/` is updated.
* Git diff has been reviewed.
* The feature is committed.

---

# 31. Git Workflow

Use focused and meaningful commits.

Examples:

```text
feat(customers): add customer entity

feat(customers): add customer repository

feat(customers): add create customer command

feat(customers): add customer queries

feat(customers): add customer authorization

test(customers): add customer integration tests

docs(customers): document customer management
```

A complete feature may also be committed as one logical commit:

```text
feat(customers): implement customer management
```

Documentation and memory changes should normally be included in the feature commit.

---

# 32. Git Safety

Before committing:

```text
git status
git diff
```

Review all changed files.

Never commit:

* Passwords
* JWT signing keys
* API keys
* User secrets
* Database credentials
* Private certificates
* Sensitive environment configuration

---

# 33. Working With Existing Code

Before modifying existing code:

1. Read the implementation.
2. Read related tests.
3. Read relevant documentation.
4. Read relevant memory.
5. Understand existing architectural decisions.
6. Follow established conventions.

Do not rewrite working code without a clear reason.

Do not introduce unrelated refactoring while implementing a feature.

Do not migrate existing functionality to CQRS merely for consistency unless the current task requires it.

---

# 34. Current Project Status

## Epic 1 — Authentication & Authorization

Main functionality:

* ASP.NET Core Identity
* ApplicationUser
* Role seeding
* Admin seeding
* JWT authentication
* Registration
* Login
* Current user endpoint
* Role-based authorization
* Integration tests

Roles:

```text
Admin
SalesEmployee
Customer
```

---

## Epic 2 — Customers

User Story:

> As a sales employee, I want to manage customers so that customers can place and track orders.

Tasks:

```text
- Create Customer entity
- Define customer properties
- Create customer profile relationship with ApplicationUser
- Create customer DTOs
- Create customer validators
- Implement customer creation
- Implement customer retrieval
- Implement customer update
- Implement customer activation/deactivation
- Implement customer listing
- Add pagination
- Add search
- Add authorization
- Add unit tests
- Add integration tests
- Update API documentation
```

CQRS candidates:

```text
Commands:
- CreateCustomerCommand
- UpdateCustomerCommand
- ActivateCustomerCommand
- DeactivateCustomerCommand

Queries:
- GetCustomerByIdQuery
- GetCustomersQuery
- SearchCustomersQuery
```

These names are examples and should be adapted to the final implementation.

Definition of Done:

```text
- Sales employee can manage customers
- Customers cannot access another customer's private data
- Validation works
- Authorization works
- Tests pass
- Documentation is updated
- Memory is updated
```

---

# 35. Mandatory Documentation & Memory Rule

**After every completed feature, both `docs/` and `memory/` MUST be reviewed and updated before the feature is considered complete.**

Required workflow:

```text
Feature
   ↓
Domain
   ↓
CQRS Command / Query
   ↓
Handlers
   ↓
Repository
   ↓
Unit of Work
   ↓
API
   ↓
Tests
   ↓
Verification
   ↓
Coverage Analysis
   ↓
docs/
   ↓
memory/
   ↓
Git Diff Review
   ↓
Git Commit
```

The repository should contain enough documentation and memory for another AI agent or developer to continue development without depending on previous chat history.

---

# 36. Core Architectural Rules

The following rules are especially important for OrderFlow:

```text
1. Follow Clean Architecture.
2. Use CQRS to separate commands from queries.
3. Use Repository Pattern for persistence abstraction.
4. Use Unit of Work to coordinate persistence changes.
5. Keep controllers thin.
6. Keep business rules out of controllers.
7. Do not expose EF Core entities directly through the API.
8. Do not inject DbContext directly into normal Application feature code.
9. Use EF Core efficiently.
10. Use optimized queries for read operations.
11. Use AsNoTracking for appropriate read-only queries.
12. Use database-level pagination.
13. Protect resources with authorization.
14. Customers must only access their own private data.
15. Write meaningful tests.
16. Analyze coverage when appropriate.
17. Update docs after every completed feature.
18. Update memory after every completed feature.
19. Review git diff before committing.
20. Commit completed features with meaningful commit messages.
21. Do not over-engineer.
22. Do not introduce patterns without a real reason.
```
