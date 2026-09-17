# OrderFlow Postman Collection

## Files

- `OrderFlow.postman_collection.json` — authentication and Epic 2 customer-management requests.
- `OrderFlow.postman_environment.json` — local URL, credentials, JWT, customer ID, and pagination variables.

## Import and setup

1. Start the API with the HTTP development profile so it listens on `http://localhost:5000`.
2. Import both JSON files into Postman.
3. Select the `OrderFlow Local` environment.
4. Set `adminPassword` to the configured admin seed password if you want to call admin-only requests.
5. Authenticate a SalesEmployee or Admin user and copy its access token into `jwtToken` for management requests.

## Suggested flow

1. Run `Authentication → Register Customer`.
2. Run `Authentication → Login`; the test script stores the response token in `customerToken`.
3. Run `Authentication → Current User` and `Customers — Customer Role → Get Current Customer`.
4. For customer management, set `jwtToken` to a SalesEmployee/Admin JWT, then run `Customers — Sales/Admin → Create Customer`.
5. The create request stores the returned profile ID in `customerId`; use it for get, update, activate, and deactivate requests.
6. Configure `searchTerm`, `isActive`, `pageNumber`, and `pageSize` before running `List Customers`.

The collection reflects the current API routes. Registration and login are public; `/api/auth/me` requires authentication; admin authorization requires the Admin role; customer management requires Admin or SalesEmployee except for owner-protected customer reads.
