# Authentication & Authorization

Strategy rationale: `../architecture/adr/003-authentication-strategy.md`.
Hardening details (lockout, JWT validation, rate limiting):
`auth-hardening.md`.

## How it works

- **Identity**: ASP.NET Core Identity stores users (`ApplicationUser` with
  `FullName`) and roles; passwords are hashed, never logged. Startup seeds
  the `Admin`, `SalesEmployee`, and `Customer` roles plus an admin user from
  configuration (`AdminSeed__*`, env-supplied in containers).
- **Tokens**: login and registration return a JWT access token (issuer,
  audience, ≥32-char signing key, 60-minute expiry, zero clock skew).
  Email confirmation is an explicit future decision — registration issues
  tokens immediately.

## Endpoint matrix

| Endpoint | Access |
|---|---|
| `POST /api/auth/register`, `POST /api/auth/login` | anonymous (rate-limited) |
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
