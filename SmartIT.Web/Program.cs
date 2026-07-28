using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using QuestPDF.Infrastructure;
using Serilog;
using SmartIT.Application;
using SmartIT.Infrastructure;
using SmartIT.Web.Hubs;

QuestPDF.Settings.License = LicenseType.Community;
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/smartit-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/smartit-.log", rollingInterval: RollingInterval.Day));

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
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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
    }

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHub<NotificationHub>("/hubs/notifications");
    app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

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
