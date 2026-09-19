# Frontend (Angular 20 + PrimeNG)

Customer shop and staff backoffice in `frontend/`. Signals + services for
state (no NgRx); PrimeNG Aura components; `X-Correlation-ID` on every
request; silent single-flight token refresh with session restore on reload.

## Run it

```powershell
cd frontend
npm ci
npm start            # ng serve on :4200, API from public/config.json
npm test             # Karma + Edge/Chrome headless (CHROME_BIN)
npm run e2e          # Playwright; needs the compose stack + Mailpit
```

With Docker: `docker compose up --build` also serves the SPA on :4200
(`API_URL` env points it at the API). Mailpit UI: :8025.

## Design system

Visual direction lives in `design-system/orderflow/MASTER.md` (emerald
primary, orange conversion CTA, Rubik headings, Nunito Sans body). PrimeNG
Aura is customized through `app/theme/preset.ts`; dark mode works through
the CSS `color-scheme` property (PrimeNG v20 resolves `light-dark()` from
it, so the `.app-dark` toggle must set `color-scheme: dark` on `html` —
never pin `color-scheme` on `body`, which would freeze the whole subtree).

## Conventions

- One type per file, including `core/models/*`.
- Backend error codes map to messages in `ApiErrorHandler`; order statuses
  map centrally in `order-status.ts`.
- Checkout generates one idempotency key per cart content and replays the
  backend `Idempotency-Key` contract.
- E2E (`e2e/shop-flow.spec.ts`) registers, confirms via Mailpit, shops,
  orders, and cancels against the real stack.
