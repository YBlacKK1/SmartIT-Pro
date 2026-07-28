# SmartIT Pro — Phase 2 Review

## Implemented

- Added MediatR-based CQRS for Assets, Employees and Tickets.
- Added feature-specific persistence ports instead of exposing generic repositories to those modules.
- Added FluentValidation pipeline behavior for all commands.
- Added API validation exception handling with RFC-compatible validation problem responses.
- Reworked update handlers to load and mutate tracked entities, preventing audit/navigation data loss.
- Added uniqueness checks for asset tags, serial numbers and employee emails.
- Added department/requester existence checks.
- Normalized asset tags and employee emails before persistence.
- Added deterministic ordering and `AsNoTracking` to read queries.
- Hardened profile photo uploads with size, MIME type, extension and randomized filename checks.
- Preserved current MVC Razor models to avoid breaking the UI.
- Added handler-level unit tests.

## Remaining legacy usage

The generic repository remains temporarily for Operations, Reports and Dashboard compatibility. It should be removed in Phase 3 after those modules receive dedicated query/command handlers.

## Recommended Phase 3

1. Asset assignment and return workflows as transactional commands.
2. Audit logging through a MediatR pipeline/domain event rather than controller code.
3. Replace DashboardService and ReportsController repository reads with optimized projections.
4. Introduce optimistic concurrency (`rowversion`) for Assets, Employees and Tickets.
5. Add integration tests using SQL Server/Testcontainers rather than EF Core InMemory.
