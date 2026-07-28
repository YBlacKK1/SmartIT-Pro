# SmartIT Pro

Enterprise ASP.NET Core 8 IT asset management and service desk solution using Clean Architecture.

## Projects

| Project | Purpose |
| --- | --- |
| SmartIT.Domain | Business entities and enums |
| SmartIT.Application | DTOs, contracts, AutoMapper, FluentValidation |
| SmartIT.Infrastructure | EF Core SQL Server, Identity, repository, data seed |
| SmartIT.Web | Bootstrap MVC admin panel, reports, QR, SignalR |
| SmartIT.API | Swagger-enabled REST API |
| SmartIT.Tests | Unit tests |

## Run

Install .NET 8 SDK and SQL Server/LocalDB, configure `DefaultConnection` in both hosts, then run:

```powershell
dotnet restore
dotnet build SmartIT.sln
dotnet test SmartIT.Tests
dotnet run --project SmartIT.Web
```

The first run creates schema through EF Core and seeds `Admin`/`User` roles and an administrator: `admin@smartit.local` / `Admin123!`. Change that password immediately. Run the API project and browse to `/swagger` for the REST contract.

Included modules: dashboard, employee profiles/photos/departments, asset inventory, assignment history domain model, ticket/comments/attachments domain model, licenses, maintenance, audit logs, reports (CSV/XLSX/PDF), QR labels, Identity roles, SignalR notifications, dark mode, Serilog, validation, tests, and sample data.

Use [INSTALLATION.md](INSTALLATION.md) for detailed database deployment notes.
