# Deploying to MonsterASP.NET

## Compatibility

MonsterASP supports .NET 10 with the default InProcess IIS hosting model,
which is what this project publishes. Publish **framework-dependent** (the
`dotnet publish` default): EXE/self-contained binaries with OutOfProcess
hosting are not supported and get the application pool disabled.

## Publish

```powershell
dotnet publish src/OrderFlow.API/OrderFlow.API.csproj -c Release
```

Deploy the publish output via Visual Studio publish profile, FTP, or Web
Deploy, as usual for the account.

## Required settings (control panel app settings / environment)

| Setting | Notes |
|---|---|
| `ConnectionStrings__DefaultConnection` | Panel-provided SQL Server string, verbatim |
| `Jwt__SecretKey` | Random string, at least 32 characters |
| `AdminSeed__Email` / `AdminSeed__Password` | Initial admin; password applies on first seed |
| `Email__Enabled` + `Email__Host/Port/From/UserName/Password` | Production startup **fails** unless email is fully configured; use the host's SMTP relay |
| `Cors__AllowedOrigins` (optional) | JSON array, e.g. `["https://shop.example.com"]`; unset = no CORS |

`ASPNETCORE_ENVIRONMENT` should be `Production` (Scalar/OpenAPI stay
unmapped; JSON Console + rolling file logs under `App_Data/logs/`).

## First start

Migrations and role/admin seeding run automatically. Data Protection keys
persist under `App_Data/keys`, so app-pool recycles do not invalidate
Identity tokens. SQL connections use transient-fault retries.

## Background jobs caveat

Hangfire and the outbox dispatcher only run while the app pool is awake;
shared-hosting idle timeout (~20 min) suspends them. Point an external
uptime monitor at `GET /health/live` to keep the pool warm — pending
notifications and accounting syncs flush on wake. `/health/ready`
(includes the database check) is the readiness probe.
