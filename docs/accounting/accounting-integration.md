# Accounting integration

Confirmed orders produce an accounting outbox message. The API order response exposes synchronization status, external invoice reference, attempt count, and last error for operational visibility. The Hangfire job sends an invoice to the configured external accounting API through the typed `IAccountingService` client.

## Configuration

```json
{
  "Accounting": {
    "Provider": "Fake",
    "BaseUrl": "http://localhost:5099/",
    "InvoicesPath": "api/invoices",
    "ApiKey": null,
    "TimeoutSeconds": 10,
    "RetryCount": 2
  }
}
```

`Provider` selects the implementation behind `IAccountingService`:

- `Fake` (default): the local stub below. Same HTTP contract as a real
  vendor, so swapping later is config-only.
- `GenericRest`: any real REST vendor speaking the same invoice contract —
  set `BaseUrl`, `InvoicesPath`, and `ApiKey` (sent as `X-Api-Key`).
  Use environment variables or a secret store for production values.
- `Disabled`: orders complete without invoicing; a synthetic
  `DISABLED-{orderId}` reference is recorded with a warning log.

The client retries transient HTTP 5xx and network/timeout failures with a bounded retry count; Hangfire provides the outer three-attempt retry. Non-transient 4xx responses fail immediately.

## Idempotency and status

The idempotency key is `Order:{orderId}:Invoice`. Order state records `Pending`, `Succeeded`, or `Failed`, the external invoice ID, attempt count, last attempt time, and the last error. Repeated requests with the same key return the same invoice from the fake provider and completed synchronization is skipped.

## Fake provider

Run `src/OrderFlow.FakeAccountingApi` on port `5099`. It exposes `POST /api/invoices`, stores invoices in memory, and supports deterministic failure testing:

- `POST /api/invoices?mode=fail` returns HTTP 500.
- `POST /api/invoices?mode=timeout` delays until the caller times out.
- Reusing an `idempotencyKey` returns the original invoice ID.
