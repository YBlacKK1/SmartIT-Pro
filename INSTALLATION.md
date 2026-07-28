# Installation guide

Requirements: .NET 8 SDK, SQL Server 2019+ or LocalDB, and NuGet internet access.

1. Update the `DefaultConnection` setting in `SmartIT.Web/appsettings.json` and `SmartIT.API/appsettings.json`.
2. Run `dotnet restore`, `dotnet build SmartIT.sln`, and `dotnet test SmartIT.Tests` from the solution root.
3. Run `dotnet run --project SmartIT.Web`; sign in with the seeded administrator account.
4. Run `dotnet run --project SmartIT.API` to expose Swagger at `/swagger`.

`database/CreateDatabase.sql` is the reviewable SQL Server business schema. In a normal application deployment, let EF Core create and migrate the database on first start, including ASP.NET Identity tables. For a change-managed rollout, create an EF migration with `dotnet ef migrations add InitialCreate --project SmartIT.Infrastructure --startup-project SmartIT.Web`, review it, then apply it with `dotnet ef database update --project SmartIT.Infrastructure --startup-project SmartIT.Web`.
