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
(`API_URL` env points it at the API). Mailpit UI: :8025. `nginx.conf`
sends `no-cache` for `index.html`/favicons and immutable year-long
caching for hashed bundles, so redeploys show up on next reload.

## Shop upgrades (2026-09-26)

- `app/shared/product-art`: deterministic per-category gradient art with a
  keyword icon and initials chip (compact variant for cart thumbs), used
  on catalog cards, product detail, and cart lines.
- Add-to-cart fires a `cartAdded` toast with a **View cart** action. The
  shell renders toasts through a `headless` template, which requires
  `PrimeTemplate` in the shell's own imports to register.
- Order detail shows a 4-step tracking timeline (Submitted → Confirmed →
  Processing → Completed) with `aria-current`, vertical on mobile;
  terminal Rejected/Cancelled states keep the danger tag.

## Role-aware navigation (2026-09-26)

Shop pages (`/catalog`, `/cart`, `/orders`) are Customer-only on the API
(403 otherwise), so the shell only links them for customers; staff land on
the backoffice links. 401/403 failures map to plain-language messages in
`ApiErrorHandler` instead of the generic fallback.

## Responsiveness & loading (2026-09-26)

- Every list page (shop orders, admin products/customers/orders,
  dashboard) shows a `role=status` loading indicator instead of a blank
  screen; tables have empty messages.
- Cart uses the wrapping `.line-row` layout with labelled quantity
  steppers; detail/checkout/inventory buttons wrap; long names/SKUs wrap
  (`overflow-wrap: anywhere`).
- Admin dialogs are width-capped with a `95vw` mobile breakpoint and
  full-width stacked forms; table action cells wrap (`.row-actions`).
- Prices render via `CurrencyPipe` everywhere; dashboard stat values clamp.

## Egypt localization (2026-09-26)

- Currency is EGP everywhere (`currency:'EGP'`); e2e subtotal assertion is
  format-agnostic.
- `LangService` (`orderflow_lang`, default `en`): header عربي/EN toggle,
  `document.lang`/`dir` switching, full shop + auth + shell dictionary
  (`i18n-dictionary.ts`, ~140 keys). Backoffice stays English.
- Product photos: `Product.ImageUrl` (migration `AddProductImageUrl`),
  `POST /api/products/{id}/image` (staff only, JPEG/PNG magic-byte check,
  5 MB, served from `wwwroot/product-images`), admin upload in the
  product dialog, `ProductArt` photo-with-fallback rendering.
- Seed: `CatalogSeeder` (24 door-accessory products, 6 Arabic categories,
  EGP prices, opening stock; idempotent; skipped in Testing).
- RTL lesson: never hide content with large negative offsets — in RTL the
  scroll origin starts right, so `left:-9999px` produced an ~11kpx canvas
  and a blank viewport. Skip-link now uses the clip pattern.

## Design system polish (2026-09-26)

- Token system in `styles.scss`: `--bg-*`, `--text-*`, `--border-subtle`,
  `--of-green`, `--of-accent`, radii and shadows, with dark variants under
  `html.app-dark`. Components use tokens instead of hardcoded colors.
- Page container: `--of-page-max: 80rem` with 24px gutters. The rule must
  live in **global** styles as `main.content > router-outlet + *` —
  component encapsulation does not cross the router-outlet boundary, so a
  shell-scoped container rule never matches (pages rendered full-bleed).
- Hero: compact two-column grid, `clamp()` headline, 48px CTAs, dashboard
  order card (border, tabular figures, status pill), pill section with
  40px chips; stacked single column under 768px.

## Home page showcase (2026-09-26)

Beyond spacing: hero stat card replaced with a live-order mock (status
pill, progress bar, steps, lines, total), plus a popular-searches chip
strip, a 3-step "How it works" section, and a 3-quote testimonial grid —
all responsive with dark-mode coverage. Component style budget raised to
8kB warning / 12kB error (`angular.json`) for the landing page.

## Home polish + favicon (2026-09-26)

- Brand: `app/shared/logo` component (emerald tile with a white delivery
  route ending in an orange node + wordmark) used in the header and in the
  `app/shared/auth-aside` brand panel; ink token `--of-ink: #0B1F17`.
- Auth pages use a split layout (`.auth-split`): brand aside beside the
  form, stacking on mobile. Page width capped at `--of-page-max: 64rem`.
  `public/app-icon.svg` + `public/app-icon.ico` (plus a brand
  `favicon.ico` for automatic requests) carry the same mark, referenced
  in `index.html` plus `apple-touch-icon`. Filenames are versioned
  (`app-icon.*`) because browsers pin favicons aggressively.
- Home rhythm reworked: clamped hero padding, `36rem` copy measure,
  pill/h1/lede/action spacing scale, trust row with top divider, stat card
  with dividers and `1.25rem` radius, features section header
  (eyebrow + h2 + sub), roomier CTA band, staggered rise-in animation
  (disabled under `prefers-reduced-motion`), tuned `48rem` breakpoint.

## Storefront UX (2026-09-26)

- Catalog: 300 ms debounced search, client-side sort (name/price),
  category chips derived from the loaded page, in-stock toggle, result
  count, breadcrumb, 8-card skeletons, currency-formatted prices, and an
  empty state with one-click filter reset.
- Shell: promo bar, skip-to-catalog link, cart count badge on desktop and
  mobile nav, 4-column footer with a (front-end only) newsletter signup.
- All routes are lazy `loadComponent` chunks (verified via `ng build`).

## Conventions

- One type per file, including `core/models/*`.
- Backend error codes map to messages in `ApiErrorHandler`; order statuses
  map centrally in `order-status.ts`.
- Checkout generates one idempotency key per cart content and replays the
  backend `Idempotency-Key` contract.
- E2E (`e2e/shop-flow.spec.ts`) registers, confirms via Mailpit, shops,
  orders, and cancels against the real stack.
