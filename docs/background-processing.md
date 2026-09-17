# Background Processing

OrderFlow uses **Hangfire** with SQL Server storage for background jobs. This provides independent execution from HTTP requests, persistent job state, automatic retries, and failed-job visibility through the Hangfire dashboard.

## Transactional outbox

Order status notifications and accounting synchronization are first written to the `OutboxMessages` table in the same EF Core unit of work as the order change. `OutboxDispatcher` then publishes pending messages to Hangfire and marks them processed. This prevents a successful order transaction from losing its required background work. Dispatch is at-least-once: a process crash after Hangfire enqueue and before marking the row can produce a duplicate, so consumers must remain idempotent.

## Jobs

- Order creation and every status transition write an order notification outbox message.
- Order completion also writes an accounting synchronization outbox message.
- The dispatcher publishes both message types to Hangfire.

Jobs are implemented in `OrderFlow.Infrastructure.BackgroundProcessing` and are scheduled through the Application abstraction `IBackgroundJobScheduler`, keeping Hangfire out of Application handlers.

## Retry and failure handling

Both jobs use Hangfire's `AutomaticRetry` with three attempts. After retries are exhausted, Hangfire moves the job to the failed state and retains the exception and execution history. Jobs log successful completion with structured order IDs.

## Monitoring

Administrators and sales employees can access the authenticated Hangfire dashboard at `/hangfire`. The dashboard exposes enqueued, processing, scheduled, succeeded, and failed jobs. Dashboard access is role protected and should be additionally restricted at the network/reverse-proxy layer in production.

Testing uses the existing SQLite in-memory provider, so the scheduler is replaced with a no-op implementation in that environment. Production uses the configured SQL Server connection string for Hangfire storage and applies EF migrations at startup. Email must be explicitly enabled and fully configured in production.
