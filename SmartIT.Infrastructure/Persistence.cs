using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmartIT.Application;
using SmartIT.Application.Assets;
using SmartIT.Application.Employees;
using SmartIT.Application.Tickets;
using SmartIT.Domain;

namespace SmartIT.Infrastructure;

public sealed class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}

public sealed class SmartITDbContext(DbContextOptions<SmartITDbContext> options)
    : IdentityDbContext<ApplicationUser>(options), IUnitOfWork
{
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetAssignment> AssetAssignments => Set<AssetAssignment>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();
    public DbSet<SoftwareLicense> SoftwareLicenses => Set<SoftwareLicense>();
    public DbSet<MaintenanceSchedule> MaintenanceSchedules => Set<MaintenanceSchedule>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Department>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<Employee>(entity =>
        {
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.JobTitle).HasMaxLength(150);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasOne(x => x.Department)
                .WithMany(x => x.Employees)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Asset>(entity =>
        {
            entity.Property(x => x.AssetTag).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Manufacturer).HasMaxLength(100);
            entity.Property(x => x.Model).HasMaxLength(100);
            entity.Property(x => x.SerialNumber).HasMaxLength(100);
            entity.HasIndex(x => x.AssetTag).IsUnique();
            entity.HasIndex(x => x.SerialNumber)
                .IsUnique()
                .HasFilter("[SerialNumber] IS NOT NULL");
        });

        builder.Entity<AssetAssignment>(entity =>
        {
            entity.HasOne(x => x.Asset)
                .WithMany(x => x.Assignments)
                .HasForeignKey(x => x.AssetId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Employee)
                .WithMany(x => x.Assignments)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.AssetId, x.ReturnedAt });
        });

        builder.Entity<Ticket>(entity =>
        {
            entity.Property(x => x.Number).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Subject).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000).IsRequired();
            entity.HasIndex(x => x.Number).IsUnique();
            entity.HasIndex(x => new { x.Status, x.CreatedAt });
        });

        builder.Entity<TicketComment>(entity =>
        {
            entity.Property(x => x.Author).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Body).HasMaxLength(4000).IsRequired();
        });

        builder.Entity<TicketAttachment>(entity =>
        {
            entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.StoredPath).HasMaxLength(500).IsRequired();
        });

        builder.Entity<SoftwareLicense>(entity =>
        {
            entity.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.LicenseKey).HasMaxLength(500).IsRequired();
        });

        builder.Entity<MaintenanceSchedule>(entity =>
        {
            entity.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            entity.HasIndex(x => new { x.Completed, x.ScheduledFor });
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.Property(x => x.UserName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EntityName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.IpAddress).HasMaxLength(45);
            entity.HasIndex(x => x.CreatedAt);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
                entry.Entity.UpdatedAt = null;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.CreatedAt).IsModified = false;
                entry.Entity.UpdatedAt = utcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}


public sealed class AssetRepository(SmartITDbContext dbContext) : IAssetRepository
{
    public async Task<IReadOnlyList<Asset>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Assets.AsNoTracking().OrderBy(x => x.AssetTag).ToListAsync(cancellationToken);
    public Task<Asset?> GetTrackedAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Assets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<Asset?> GetReadOnlyAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Assets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<bool> AssetTagExistsAsync(string assetTag, Guid? excludingId, CancellationToken cancellationToken) =>
        dbContext.Assets.AnyAsync(x => x.AssetTag == assetTag && (!excludingId.HasValue || x.Id != excludingId.Value), cancellationToken);
    public Task<bool> SerialNumberExistsAsync(string serialNumber, Guid? excludingId, CancellationToken cancellationToken) =>
        dbContext.Assets.AnyAsync(x => x.SerialNumber == serialNumber && (!excludingId.HasValue || x.Id != excludingId.Value), cancellationToken);
    public async Task AddAsync(Asset asset, CancellationToken cancellationToken) => await dbContext.Assets.AddAsync(asset, cancellationToken);
}

public sealed class EmployeeRepository(SmartITDbContext dbContext) : IEmployeeRepository
{
    public async Task<IReadOnlyList<Employee>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Employees.AsNoTracking().Include(x => x.Department).OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync(cancellationToken);
    public Task<Employee?> GetTrackedAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Employees.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<Employee?> GetReadOnlyAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Employees.AsNoTracking().Include(x => x.Department).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<bool> EmailExistsAsync(string email, Guid? excludingId, CancellationToken cancellationToken) =>
        dbContext.Employees.AnyAsync(x => x.Email == email && (!excludingId.HasValue || x.Id != excludingId.Value), cancellationToken);
    public Task<bool> DepartmentExistsAsync(Guid departmentId, CancellationToken cancellationToken) =>
        dbContext.Departments.AnyAsync(x => x.Id == departmentId, cancellationToken);
    public async Task AddAsync(Employee employee, CancellationToken cancellationToken) => await dbContext.Employees.AddAsync(employee, cancellationToken);
    public void Remove(Employee employee) => dbContext.Employees.Remove(employee);
}

public sealed class DepartmentRepository(SmartITDbContext dbContext) : IDepartmentRepository
{
    public async Task<IReadOnlyList<Department>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Departments.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);
}

public sealed class TicketRepository(SmartITDbContext dbContext) : ITicketRepository
{
    public async Task<IReadOnlyList<Ticket>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Tickets.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
    public Task<Ticket?> GetReadOnlyAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Tickets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<bool> RequesterExistsAsync(Guid requesterId, CancellationToken cancellationToken) =>
        dbContext.Employees.AnyAsync(x => x.Id == requesterId, cancellationToken);
    public Task<string> GenerateNextNumberAsync(CancellationToken cancellationToken) =>
        Task.FromResult($"INC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..22].ToUpperInvariant());
    public async Task AddAsync(Ticket ticket, CancellationToken cancellationToken) => await dbContext.Tickets.AddAsync(ticket, cancellationToken);
}

public sealed class Repository<T>(SmartITDbContext dbContext) : IRepository<T> where T : Entity
{
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Set<T>().AsNoTracking().ToListAsync(cancellationToken);

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Set<T>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await dbContext.Set<T>().AddAsync(entity, cancellationToken);

    public void Update(T entity) => dbContext.Set<T>().Update(entity);

    public void Remove(T entity) => dbContext.Set<T>().Remove(entity);
}

public sealed class DashboardService(SmartITDbContext dbContext, AutoMapper.IMapper mapper) : IDashboardService
{
    public async Task<DashboardDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var recentTickets = await dbContext.Tickets
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        var assetStatus = await dbContext.Assets
            .AsNoTracking()
            .GroupBy(x => x.Status)
            .ToDictionaryAsync(x => x.Key.ToString(), x => x.Count(), cancellationToken);

        var employees = await dbContext.Employees.CountAsync(cancellationToken);
        var assets = await dbContext.Assets.CountAsync(cancellationToken);
        var openTickets = await dbContext.Tickets.CountAsync(x => x.Status != TicketStatus.Closed, cancellationToken);
        var assignedAssets = await dbContext.Assets.CountAsync(x => x.Status == AssetStatus.Assigned, cancellationToken);

        return new DashboardDto(
            employees,
            assets,
            openTickets,
            assignedAssets,
            mapper.Map<IReadOnlyCollection<TicketDto>>(recentTickets),
            assetStatus);
    }
}

public sealed class SeedOptions
{
    public const string SectionName = "Seed";
    public string AdminEmail { get; init; } = string.Empty;
    public string AdminPassword { get; init; } = string.Empty;
    public string AdminDisplayName { get; init; } = "System Administrator";
}

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<SmartITDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(5)));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<SmartITDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));
        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<SmartITDbContext>());
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<SmartITDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var roleName in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException($"Role '{roleName}' could not be created: {string.Join(", ", roleResult.Errors.Select(x => x.Description))}");
                }
            }
        }

        var seedOptions = services.GetRequiredService<IOptions<SeedOptions>>().Value;
        if (!string.IsNullOrWhiteSpace(seedOptions.AdminEmail) && !string.IsNullOrWhiteSpace(seedOptions.AdminPassword))
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var admin = await userManager.FindByEmailAsync(seedOptions.AdminEmail);

            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    UserName = seedOptions.AdminEmail,
                    Email = seedOptions.AdminEmail,
                    EmailConfirmed = true,
                    DisplayName = seedOptions.AdminDisplayName
                };

                var userResult = await userManager.CreateAsync(admin, seedOptions.AdminPassword);
                if (!userResult.Succeeded)
                {
                    throw new InvalidOperationException($"Admin user could not be created: {string.Join(", ", userResult.Errors.Select(x => x.Description))}");
                }
            }

            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        if (await dbContext.Departments.AnyAsync(cancellationToken))
        {
            return;
        }

        var it = new Department { Name = "Information Technology" };
        var finance = new Department { Name = "Finance" };
        var employee = new Employee
        {
            FirstName = "Ayşe",
            LastName = "Yılmaz",
            Email = "ayse.yilmaz@smartit.local",
            JobTitle = "IT Specialist",
            Department = it
        };
        var asset = new Asset
        {
            AssetTag = "LAP-0001",
            Name = "Dell Latitude 7440",
            Type = AssetType.Laptop,
            Manufacturer = "Dell",
            Model = "Latitude 7440",
            SerialNumber = "DL7440-001",
            Status = AssetStatus.Assigned
        };

        dbContext.Departments.AddRange(it, finance);
        dbContext.Employees.Add(employee);
        dbContext.Assets.Add(asset);
        dbContext.AssetAssignments.Add(new AssetAssignment
        {
            Asset = asset,
            Employee = employee,
            Notes = "Initial onboarding assignment"
        });
        dbContext.Tickets.Add(new Ticket
        {
            Number = "INC-000001",
            Subject = "VPN access request",
            Description = "Please grant remote access.",
            Priority = TicketPriority.Medium,
            Status = TicketStatus.Open,
            Requester = employee
        });
        dbContext.SoftwareLicenses.Add(new SoftwareLicense
        {
            ProductName = "Microsoft 365 Business",
            LicenseKey = "DEMO-ONLY-NOT-A-REAL-LICENSE",
            Seats = 50,
            UsedSeats = 18,
            ExpiresAt = DateTime.UtcNow.AddYears(1)
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
