# Docker Setup (US-16)

The entire local environment (API + SQL Server + fake accounting API)
starts with Docker Compose. No Redis: the stack does not use it
(see `docs/products/catalog-performance.md`).

## Prerequisites

- Docker Desktop (daemon running) with Compose v2.
- Ports 8080 (API) free; 1433 is container-internal only.

## Start

```powershell
Copy-Item .env.example .env
# Edit .env: SA_PASSWORD, JWT_SECRET_KEY (32+ chars), ADMIN_PASSWORD
docker compose up --build -d
```

On first start the API applies EF migrations automatically and seeds
roles plus the admin user from `ADMIN_*`. SQL Server takes ~30-60 s on
first pull; the API waits for its healthcheck.

## Verify

```powershell
Invoke-RestMethod http://localhost:8080/health/ready   # database: Healthy
```

Then log in as the seeded admin (`ADMIN_EMAIL` / `ADMIN_PASSWORD`) and use
the API normally — products, inventory, orders, and accounting
synchronization (served by the `accounting` container) all work. Verified
end to end: order confirmed in-container received a real `INV-*` invoice ID,
and data survived `docker compose down` / `up` via the `mssql-data` volume.

## Configuration

All container configuration is environment-based (`docker-compose.yml`);
no secrets are baked into images (multi-stage builds, non-root `app` user).
`ASPNETCORE_ENVIRONMENT` is `Development` in compose; running as
`Production` additionally requires `Email:Enabled=true` (enforced at
startup). Serilog file logging from Development config writes inside the
container and is discarded with it; JSON Console logging comes from the
Production config.

## Data reset

```powershell
docker compose down        # containers go, mssql-data volume stays
docker compose down -v     # also deletes the database (clean slate)
```

## Admin password changes

The admin user is seeded **once**, on the very first startup. Changing
`ADMIN_PASSWORD` (or `ADMIN_EMAIL`) in `.env` afterwards does **not**
update the existing account — log in with the original password, or reset
the database with `docker compose down -v` (deletes all data) and start
again. Note `dotnet user-secrets` never reach containers; `.env` is the
only mechanism, and only at first seed.

## Troubleshooting

- On restart with existing data, the API may crash once with
  `Database 'OrderFlowDb' already exists`: SQL Server reports healthy
  before recovered databases are visible, so the migration briefly thinks
  the database is missing. The `api` service has `restart: unless-stopped`
  and comes up healthy on retry by itself.
- `SA_PASSWORD` must satisfy SQL Server complexity (8+ chars, 3 of:

- `SA_PASSWORD` must satisfy SQL Server complexity (8+ chars, 3 of:
  upper, lower, digit, symbol) or `sqlserver` never becomes healthy.
- `Jwt:SecretKey must be at least 32 characters` aborts API startup:
  set a longer `JWT_SECRET_KEY`.
- `Admin seed password is not configured` aborts API startup:
  set `ADMIN_PASSWORD`.
- First `docker compose up` downloads ~500 MB (SQL Server) + .NET images.
