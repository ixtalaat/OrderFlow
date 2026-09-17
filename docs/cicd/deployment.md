# CI/CD and Deployment (US-17)

Pipeline: `.github/workflows/ci.yml` (GitHub Actions, `ubuntu-latest`).

```text
Git Push / PR
   ↓
Build (restore, Release build, unit tests + Coverlet, TRX report)
   ↓
Integration Tests (SQLite suites, TRX report)
   ↓
Docker Build (API + accounting images; pushed to GHCR on master)
   ↓
Deploy Smoke (master only: compose up, /health/ready + admin login, down -v)
```

Each stage `needs` the previous one, so failed tests block Docker builds
and any push or smoke run.

## Test reporting

Both test jobs publish TRX reports via `dorny/test-reporter`
(checks on the PR) and upload raw TRX plus Coverlet Cobertura files as
artifacts. Coverage is diagnostic only; there is no percentage gate
(see `docs/testing/test-strategy.md`).

## Images and reproducible deployment

- Registry: `ghcr.io/ixtalaat/orderflow-api` and
  `ghcr.io/ixtalaat/orderflow-accounting`, tagged immutable `:sha` plus
  floating `:latest` (master only, via `GITHUB_TOKEN` — no extra secrets).
- To deploy a specific build on any Docker host: check out the repo at that
  commit (or set `IMAGE_PREFIX` tags), provide `.env` (see `.env.example`),
  and run `docker compose up -d`. Roll back by checking out the previous
  commit and re-running compose — the `mssql-data` volume keeps the data.

## Verifying the pipeline

- Open a pull request: all jobs except push/smoke run; merge is blocked
  while any job is red (enable branch protection on `master` requiring the
  `Build + unit tests`, `Integration tests`, and `Build + push images`
  checks).
- Failure behavior: a red test fails its job, which cancels every
  downstream job — nothing is pushed and nothing is smoke-deployed.
- The smoke job uses ephemeral generated secrets, so CI never needs real
  credentials; production-like secrets stay in the deploy host's `.env`,
  which is gitignored and never committed.
