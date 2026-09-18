# Authentication & Authorization

Strategy rationale: `../architecture/adr/003-authentication-strategy.md`.
Hardening details (lockout, JWT validation, rate limiting):
`auth-hardening.md`.

## How it works

- **Identity**: ASP.NET Core Identity stores users (`ApplicationUser` with
  `FullName`) and roles; passwords are hashed, never logged. Startup seeds
  the `Admin`, `SalesEmployee`, and `Customer` roles plus an admin user from
  configuration (`AdminSeed__*`, env-supplied in containers).
- **Tokens**: login returns a JWT access token (issuer, audience, ≥32-char
  signing key, 15-minute expiry, zero clock skew) plus an opaque refresh
  token (7 days). `POST /api/auth/refresh` rotates the pair; reuse of an
  old refresh token revokes the whole family. `POST /api/auth/revoke`
  kills a single token. See `../architecture/adr/009-session-management.md`.
- **Email confirmation**: self-registration returns no tokens and requires
  `POST /api/auth/confirm-email` (token from the confirmation email;
  `POST /api/auth/resend-confirmation` re-sends, always 200). Login is
  blocked with `403` until confirmed. Staff-provisioned accounts are
  trusted and skip confirmation.
- **Revocation**: every access token carries a `token_version` claim
  checked against the user row per request. Deactivation and erasure bump
  the version, so outstanding tokens die immediately.

## Endpoint matrix

| Endpoint | Access |
|---|---|
| `POST /api/auth/register`, `POST /api/auth/login` | anonymous (rate-limited) |
| `POST /api/auth/confirm-email`, `POST /api/auth/resend-confirmation` | anonymous (rate-limited) |
| `POST /api/auth/refresh`, `POST /api/auth/revoke` | anonymous, token possession (rate-limited) |
| `GET /api/auth/me`, `GET /api/auth/admin` | authenticated / Admin-only probe |
| `GET /api/catalog/products*` | `Customer` |
| `POST /api/orders`, `GET /api/orders*` (own) | `Customer` |
| `POST /api/products*`, `PUT`, activate/deactivate | `Admin`, `SalesEmployee` |
| `GET /api/products*` (management) | `Admin`, `SalesEmployee` |
| `GET/PATCH/POST /api/inventory*` | `Admin`, `SalesEmployee` |
| `POST/PUT/DELETE /api/pricing-rules*` | `Admin` |
| `POST/GET/PUT/activate/deactivate /api/customers*`, `PATCH tier` | `Admin`, `SalesEmployee` |
| Order `confirm/reject/processing/complete` | `Admin`, `SalesEmployee` |
| `/hangfire` dashboard | `Admin` (+ role filter) |

## Ownership rules

- Customers resolve to their own profile from the token identity; order and
  profile reads return `401` without a profile, `403` for another
  customer's data.
- Staff can view any order or customer but mutate only through the
  management endpoints above.
- Deactivating a customer blocks new logins; existing JWTs expire naturally
  (no revocation list — accepted, documented in Epic 2 notes).
