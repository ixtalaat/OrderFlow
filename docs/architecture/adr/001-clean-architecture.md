# ADR-001: Clean Architecture

## Context

OrderFlow is a portfolio backend expected to grow epic by epic (customers,
products, orders, pricing, notifications, accounting) without turning into
a tightly coupled system where database, framework, and business logic
cannot be changed or tested independently.

## Decision

Clean Architecture with four projects: Domain (entities, rules, no
dependencies), Application (CQRS, DTOs, validators, repository/unit-of-work
interfaces), Infrastructure (EF Core, Identity, Hangfire, email,
accounting client), and Api (controllers, middleware, DI). Dependencies
point inward; Infrastructure implements Application abstractions.

## Alternatives Considered

- **Layered/n-tier with direct DbContext use**: faster initially, but
  business logic spreads into controllers and becomes untestable without a
  database.
- **Vertical slice architecture**: attractive for feature isolation, but the
  portfolio goal explicitly values demonstrating Clean Architecture with
  repository and unit-of-work patterns.

## Consequences

### Positive

- Business rules are unit-testable without EF Core or HTTP.
- Persistence and external services are swappable behind interfaces.
- Epic-by-epic growth follows established layer responsibilities.

### Negative

- More projects and indirection (repositories, UoW, MediatR) than a small
  app strictly needs; justified as a deliberate portfolio demonstration.
