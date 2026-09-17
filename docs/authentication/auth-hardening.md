# Authentication hardening

- Login uses ASP.NET Core Identity password checks with lockout enabled: five failed attempts lock an account for 15 minutes.
- JWT settings are validated at startup. `Jwt:SecretKey` must be at least 32 characters, and issuer, audience, and positive expiration are required.
- Authentication responses use the configured JWT expiration rather than a hardcoded value.
- Registration cleans up the Identity user when role assignment or customer-profile persistence fails.
- Login and registration are limited to 10 requests per IP per minute. Configure a trusted proxy correctly before relying on forwarded client IPs.
- Email confirmation is not currently required; registration intentionally issues a token immediately. Add confirmation-token delivery and enforce `RequireConfirmedEmail` when the product requires verified addresses.

Secrets must be supplied through User Secrets, environment variables, or a production secret store; never commit JWT keys or passwords.
