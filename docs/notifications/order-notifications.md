# Order Notifications

SMTP delivery is configured through `Email` settings supplied by environment variables or User Secrets. When `Email:Enabled` is true, `Host`, `Port`, `From`, `UserName`, and `Password` are required and validated at startup. Production must explicitly enable email; disabled delivery is intended only for development and tests.

Order status notifications are delivered through MailKit and Hangfire. The API only enqueues a job after a successful order/status transaction; email delivery never blocks the request.

## Notification events

Notifications are queued for:

- Order submitted
- Order confirmed
- Order rejected
- Order processing
- Order completed

## Email delivery

`SmtpEmailSender` uses MailKit with the `Email` configuration section:

```json
{
  "Email": {
    "Enabled": false,
    "Host": "smtp.example.com",
    "Port": 587,
    "From": "orders@example.com",
    "UserName": "smtp-user",
    "Password": "use-user-secrets"
  }
}
```

When enabled, MailKit connects with STARTTLS, authenticates, sends the message, and disconnects. When disabled (including test environments), delivery is logged without contacting an SMTP server.

## Reliability

Hangfire retries notification jobs three times. Exceptions from MailKit are logged by Hangfire and the job is moved to the failed state after retries are exhausted. Failed notifications can be retried from the protected `/hangfire` dashboard.
