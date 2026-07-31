using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using QuestPDF.Infrastructure;
using Serilog;
using SmartIT.Application;
using SmartIT.Infrastructure;
using SmartIT.Web.Hubs;

QuestPDF.Settings.License = LicenseType.Community;

var logFilePath = ResolveLogFilePath();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    ConfigureDatabasePath(builder);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day));

    builder.Services.AddProblemDetails();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddControllersWithViews();
    builder.Services.AddSignalR();
    builder.Services.AddAuthorization();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.Name = "SmartIT.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    var app = builder.Build();

    app.UseForwardedHeaders();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
        app.UseHttpsRedirection();
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");
    app.UseSerilogRequestLogging();
    app.UseStaticFiles();
    app.UseRouting();

    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        await next();
    });

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHub<NotificationHub>("/hubs/notifications");
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    await DbInitializer.SeedAsync(app.Services);
    await app.RunAsync();
}
catch (Exception exception)
{
    Log.Fatal(exception, "SmartIT Web terminated unexpectedly.");
}
finally
{
    await Log.CloseAndFlushAsync();
}

static void ConfigureDatabasePath(WebApplicationBuilder builder)
{
    if (builder.Environment.IsDevelopment())
    {
        var localDatabase = Path.Combine(builder.Environment.ContentRootPath, "smartit-v1.db");
        builder.Configuration["ConnectionStrings:DefaultConnection"] =
            $"Data Source={localDatabase};Cache=Shared;Foreign Keys=True";
        return;
    }

    var isAzureAppService =
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
    var hasExplicitConnectionString =
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"));
    var homePath = Environment.GetEnvironmentVariable("HOME");

    if (!isAzureAppService || hasExplicitConnectionString || string.IsNullOrWhiteSpace(homePath))
    {
        return;
    }

    var dataDirectory = Path.Combine(homePath, "data");
    Directory.CreateDirectory(dataDirectory);
    var databasePath = Path.Combine(dataDirectory, "smartit-v1.db");
    builder.Configuration["ConnectionStrings:DefaultConnection"] =
        $"Data Source={databasePath};Cache=Shared;Foreign Keys=True";
}

static string ResolveLogFilePath()
{
    var isAzureAppService =
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
    var homePath = Environment.GetEnvironmentVariable("HOME");

    var logDirectory = isAzureAppService && !string.IsNullOrWhiteSpace(homePath)
        ? Path.Combine(homePath, "LogFiles", "SmartIT")
        : Path.Combine(AppContext.BaseDirectory, "logs");

    Directory.CreateDirectory(logDirectory);
    return Path.Combine(logDirectory, "smartit-.log");
}
