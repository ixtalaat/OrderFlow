# ADR-003: Authentication Strategy

## Context

Three user types (Admin, SalesEmployee, Customer) need credentials, role
checks, and customer-data ownership enforcement, with minimal custom
security code.

## Decision

ASP.NET Core Identity for user/role storage (hashed passwords, lockout)
plus JWT bearer tokens for API authentication (issuer/audience validated,
≥32-char key, 60-minute expiry, zero clock skew). Controllers enforce roles;
handlers and controllers jointly enforce resource ownership. Details:
`docs/authentication/authentication.md`.

## Alternatives Considered

- **Cookie authentication**: simpler for browsers but wrong for a stateless
  API consumed by arbitrary clients.
- **External identity provider (Entra ID, Auth0)**: removes password
  handling entirely, but adds an external dependency and cost for a
  portfolio project; the seam (token validation) allows migrating later.
- **Custom password/token code**: rejected — never roll your own auth.

## Consequences

### Positive

- Proven password hashing, lockout, and role management with no custom
  cryptography.
- Stateless scaling; every request carries identity and roles.

### Negative

- No server-side token revocation: deactivated customers keep valid JWTs
  until expiry (accepted; documented in Epic 2 notes).
- Email confirmation deferred: registration issues tokens immediately
  (explicit future decision).
