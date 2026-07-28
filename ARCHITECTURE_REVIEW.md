# SmartIT Pro — Architecture Review (Phase 1)

## Completed

- Replaced repository-level `SaveChangesAsync` with a scoped `IUnitOfWork`.
- Added explicit EF Core entity constraints, indexes, lengths, and delete behavior.
- Added automatic UTC audit timestamps in `SmartITDbContext`.
- Removed the committed default administrator password.
- Replaced `EnsureCreatedAsync` with migration-based startup.
- Hardened ASP.NET Core Identity password, lockout, and unique-email policies.
- Added secure cookie configuration for the MVC application.
- Added explicit JWT validation and Swagger bearer authentication for the API.
- Added API authorization requirements.
- Added Problem Details, request logging, startup error logging, and graceful Serilog shutdown.
- Prevented AutoMapper from overwriting audit and navigation properties.
- Added CancellationToken support to revised endpoints.

## Required before first run

Create and apply an EF Core migration:

```bash
dotnet ef migrations add InitialCreate \
  --project SmartIT.Infrastructure \
  --startup-project SmartIT.Web \
  --output-dir Persistence/Migrations

dotnet ef database update \
  --project SmartIT.Infrastructure \
  --startup-project SmartIT.Web
```

Configure secrets outside source control:

```bash
dotnet user-secrets init --project SmartIT.Web
dotnet user-secrets set "Seed:AdminEmail" "admin@example.com" --project SmartIT.Web
dotnet user-secrets set "Seed:AdminPassword" "CHANGE_WITH_A_STRONG_PASSWORD" --project SmartIT.Web

dotnet user-secrets init --project SmartIT.API
dotnet user-secrets set "Jwt:Key" "USE_AT_LEAST_32_RANDOM_BYTES" --project SmartIT.API
```

## Next architecture phase

The generic repository remains a transitional abstraction. The next phase should introduce feature-based Application use cases (Assets, Employees, Tickets), command/query handlers, dedicated request models, pagination, conflict handling, file-storage abstraction, and integration tests using SQL Server or Testcontainers.
