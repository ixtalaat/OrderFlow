# ADR-009: Session Management (Refresh, Revocation, Confirmation)

## Context

Hourly JWT expiry without refresh hurt usability; deactivated users kept
valid tokens until expiry; and registration issued tokens for unconfirmed
emails.

## Decision

- Short-lived access tokens (15 min) with rotating opaque refresh tokens
  (7 days, SHA-256 hashes stored, never the token itself).
- Rotation on every refresh; reuse of a rotated token revokes the whole
  family and bumps the user's token version (theft containment).
- `TokenVersion` claim on every access token, validated against the user
  row on each request; deactivation and erasure bump it for instant
  revocation.
- Self-registered emails must be confirmed before login (Identity
  confirmation tokens by email); staff-provisioned accounts are trusted.

## Alternatives Considered

- **Long-lived JWTs**: simpler, but a stolen token is valid for hours with
  no revocation path.
- **Server-side session store for everything**: stateful and heavier; the
  hybrid (stateless access + stored refresh) keeps hot paths fast.
- **Sliding expiration without rotation**: weaker theft detection than
  reuse-sensitive rotation.

## Consequences

### Positive

- Stolen tokens have a 15-minute blast radius; reuse is detected and
  contained; deactivation takes effect immediately.
- No unconfirmed-email accounts can transact.

### Negative

- More moving parts (refresh table, rotation, cleanup of expired rows) and
  clients must implement the refresh flow.
