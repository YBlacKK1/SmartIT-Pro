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
            entity.Property(x => x.ProfilePhotoPath).HasMaxLength(500);
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
            entity.HasIndex(x => x.SerialNumber).IsUnique();
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
            entity.HasOne(x => x.Requester)
                .WithMany()
                .HasForeignKey(x => x.RequesterId)
                .OnDelete(DeleteBehavior.SetNull);
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
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = utcNow;
                }
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

    public Task<Asset?> GetDetailsAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Assets
            .AsNoTracking()
            .Include(x => x.Assignments)
            .ThenInclude(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> AssetTagExistsAsync(string assetTag, Guid? excludingId, CancellationToken cancellationToken)
    {
        var normalized = assetTag.Trim().ToUpperInvariant();
        return dbContext.Assets.AnyAsync(x => x.AssetTag == normalized && (!excludingId.HasValue || x.Id != excludingId.Value), cancellationToken);
    }

    public Task<bool> SerialNumberExistsAsync(string serialNumber, Guid? excludingId, CancellationToken cancellationToken)
    {
        var normalized = serialNumber.Trim();
        return dbContext.Assets.AnyAsync(x => x.SerialNumber == normalized && (!excludingId.HasValue || x.Id != excludingId.Value), cancellationToken);
    }

    public async Task AddAsync(Asset asset, CancellationToken cancellationToken) =>
        await dbContext.Assets.AddAsync(asset, cancellationToken);
}

public sealed class EmployeeRepository(SmartITDbContext dbContext) : IEmployeeRepository
{
    public async Task<IReadOnlyList<Employee>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Employees.AsNoTracking().Include(x => x.Department)
            .OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync(cancellationToken);

    public Task<Employee?> GetTrackedAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Employees.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Employee?> GetReadOnlyAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Employees.AsNoTracking().Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, Guid? excludingId, CancellationToken cancellationToken)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return dbContext.Employees.AnyAsync(x => x.Email == normalized && (!excludingId.HasValue || x.Id != excludingId.Value), cancellationToken);
    }

    public Task<bool> DepartmentExistsAsync(Guid departmentId, CancellationToken cancellationToken) =>
        dbContext.Departments.AnyAsync(x => x.Id == departmentId, cancellationToken);

    public Task<bool> HasAssignmentsAsync(Guid employeeId, CancellationToken cancellationToken) =>
        dbContext.AssetAssignments.AnyAsync(x => x.EmployeeId == employeeId, cancellationToken);

    public async Task AddAsync(Employee employee, CancellationToken cancellationToken) =>
        await dbContext.Employees.AddAsync(employee, cancellationToken);

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
        await dbContext.Tickets.AsNoTracking().Include(x => x.Requester)
            .OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

    public Task<Ticket?> GetTrackedAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Tickets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Ticket?> GetReadOnlyAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Tickets.AsNoTracking().Include(x => x.Requester)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> RequesterExistsAsync(Guid requesterId, CancellationToken cancellationToken) =>
        dbContext.Employees.AnyAsync(x => x.Id == requesterId, cancellationToken);

    public Task<string> GenerateNextNumberAsync(CancellationToken cancellationToken) =>
        Task.FromResult($"INC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..22].ToUpperInvariant());

    public async Task AddAsync(Ticket ticket, CancellationToken cancellationToken) =>
        await dbContext.Tickets.AddAsync(ticket, cancellationToken);
}

public sealed class Repository<T>(SmartITDbContext dbContext) : IRepository<T> where T : Entity
{
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Set<T>().AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Set<T>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await dbContext.Set<T>().AddAsync(entity, cancellationToken);

    public void Update(T entity) => dbContext.Set<T>().Update(entity);
    public void Remove(T entity) => dbContext.Set<T>().Remove(entity);
}

public sealed class DashboardService(SmartITDbContext dbContext) : IDashboardService
{
    public async Task<DashboardDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var recentTickets = await dbContext.Tickets
            .AsNoTracking()
            .Include(x => x.Requester)
            .OrderByDescending(x => x.CreatedAt)
            .Take(6)
            .Select(x => new TicketDto(
                x.Id,
                x.Number,
                x.Subject,
                x.Description,
                x.Priority,
                x.Status,
                x.RequesterId,
                x.Requester == null ? null : x.Requester.FirstName + " " + x.Requester.LastName,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        var assetStatus = await dbContext.Assets.AsNoTracking()
            .GroupBy(x => x.Status)
            .ToDictionaryAsync(x => x.Key.ToString(), x => x.Count(), cancellationToken);

        var ticketStatus = await dbContext.Tickets.AsNoTracking()
            .GroupBy(x => x.Status)
            .ToDictionaryAsync(x => x.Key.ToString(), x => x.Count(), cancellationToken);

        return new DashboardDto(
            await dbContext.Employees.CountAsync(cancellationToken),
            await dbContext.Assets.CountAsync(cancellationToken),
            await dbContext.Tickets.CountAsync(x => x.Status == TicketStatus.Open || x.Status == TicketStatus.InProgress, cancellationToken),
            await dbContext.Assets.CountAsync(x => x.Status == AssetStatus.Assigned, cancellationToken),
            await dbContext.Assets.CountAsync(x => x.Status == AssetStatus.Available, cancellationToken),
            await dbContext.Tickets.CountAsync(x => x.Priority == TicketPriority.Critical &&
                (x.Status == TicketStatus.Open || x.Status == TicketStatus.InProgress), cancellationToken),
            await dbContext.Tickets.CountAsync(x => x.Status == TicketStatus.Resolved || x.Status == TicketStatus.Closed, cancellationToken),
            recentTickets,
            assetStatus,
            ticketStatus);
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

        services.AddDbContext<SmartITDbContext>(options => options.UseSqlite(connectionString));

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

        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var roleName in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Role '{roleName}' could not be created: {string.Join(", ", roleResult.Errors.Select(x => x.Description))}");
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
                    throw new InvalidOperationException(
                        $"Admin user could not be created: {string.Join(", ", userResult.Errors.Select(x => x.Description))}");
                }
            }

            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        if (await dbContext.Departments.AnyAsync(cancellationToken)) return;

        var it = new Department { Name = "Information Technology" };
        var finance = new Department { Name = "Finance" };
        var humanResources = new Department { Name = "Human Resources" };
        var operations = new Department { Name = "Operations" };

        var ayse = new Employee
        {
            FirstName = "Ayşe",
            LastName = "Yılmaz",
            Email = "ayse.yilmaz@smartit.local",
            JobTitle = "IT Specialist",
            Department = it
        };
        var mert = new Employee
        {
            FirstName = "Mert",
            LastName = "Kaya",
            Email = "mert.kaya@smartit.local",
            JobTitle = "Finance Analyst",
            Department = finance
        };
        var selin = new Employee
        {
            FirstName = "Selin",
            LastName = "Demir",
            Email = "selin.demir@smartit.local",
            JobTitle = "HR Business Partner",
            Department = humanResources
        };

        var laptop = new Asset
        {
            AssetTag = "LAP-0001",
            Name = "Dell Latitude 7440",
            Type = AssetType.Laptop,
            Manufacturer = "Dell",
            Model = "Latitude 7440",
            SerialNumber = "DL7440-001",
            Status = AssetStatus.Assigned,
            PurchaseDate = DateTime.UtcNow.AddMonths(-10)
        };
        var monitor = new Asset
        {
            AssetTag = "MON-0007",
            Name = "UltraSharp 27 Monitor",
            Type = AssetType.Monitor,
            Manufacturer = "Dell",
            Model = "U2723QE",
            SerialNumber = "MON-U27-007",
            Status = AssetStatus.Assigned
        };
        var switchAsset = new Asset
        {
            AssetTag = "SWT-0002",
            Name = "Core Network Switch",
            Type = AssetType.Switch,
            Manufacturer = "Cisco",
            Model = "Catalyst 9200",
            SerialNumber = "CSC-9200-002",
            Status = AssetStatus.Available
        };
        var printer = new Asset
        {
            AssetTag = "PRN-0004",
            Name = "Office Laser Printer",
            Type = AssetType.Printer,
            Manufacturer = "HP",
            Model = "LaserJet Pro",
            SerialNumber = "HP-LJ-004",
            Status = AssetStatus.InMaintenance
        };
        var phone = new Asset
        {
            AssetTag = "PHN-0012",
            Name = "Corporate Smartphone",
            Type = AssetType.Phone,
            Manufacturer = "Samsung",
            Model = "Galaxy S24",
            SerialNumber = "SG-S24-012",
            Status = AssetStatus.Available
        };

        dbContext.Departments.AddRange(it, finance, humanResources, operations);
        dbContext.Employees.AddRange(ayse, mert, selin);
        dbContext.Assets.AddRange(laptop, monitor, switchAsset, printer, phone);
        dbContext.AssetAssignments.AddRange(
            new AssetAssignment { Asset = laptop, Employee = ayse, Notes = "Primary work device" },
            new AssetAssignment { Asset = monitor, Employee = mert, Notes = "Finance workstation" });

        dbContext.Tickets.AddRange(
            new Ticket
            {
                Number = "INC-000001",
                Subject = "VPN access request",
                Description = "Remote connection fails after the latest password change.",
                Priority = TicketPriority.High,
                Status = TicketStatus.InProgress,
                Requester = ayse,
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            },
            new Ticket
            {
                Number = "INC-000002",
                Subject = "Printer queue is offline",
                Description = "The finance floor printer does not accept new jobs.",
                Priority = TicketPriority.Critical,
                Status = TicketStatus.Open,
                Requester = mert,
                CreatedAt = DateTime.UtcNow.AddHours(-7)
            },
            new Ticket
            {
                Number = "INC-000003",
                Subject = "New employee account setup",
                Description = "Prepare email, VPN and shared folder permissions.",
                Priority = TicketPriority.Medium,
                Status = TicketStatus.Resolved,
                Requester = selin,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Ticket
            {
                Number = "INC-000004",
                Subject = "Monitor flickering",
                Description = "External monitor flickers intermittently over USB-C.",
                Priority = TicketPriority.Low,
                Status = TicketStatus.Closed,
                Requester = ayse,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            });

        dbContext.SoftwareLicenses.AddRange(
            new SoftwareLicense
            {
                ProductName = "Microsoft 365 Business Premium",
                LicenseKey = "DEMO-ONLY-NOT-A-REAL-LICENSE",
                Seats = 50,
                UsedSeats = 31,
                ExpiresAt = DateTime.UtcNow.AddMonths(9)
            },
            new SoftwareLicense
            {
                ProductName = "JetBrains All Products Pack",
                LicenseKey = "DEMO-ONLY-NOT-A-REAL-LICENSE-2",
                Seats = 10,
                UsedSeats = 7,
                ExpiresAt = DateTime.UtcNow.AddMonths(5)
            });

        dbContext.MaintenanceSchedules.AddRange(
            new MaintenanceSchedule
            {
                Asset = printer,
                Description = "Replace maintenance kit and inspect rollers",
                ScheduledFor = DateTime.UtcNow.AddDays(3),
                Completed = false
            },
            new MaintenanceSchedule
            {
                Asset = switchAsset,
                Description = "Quarterly firmware and configuration backup",
                ScheduledFor = DateTime.UtcNow.AddDays(14),
                Completed = false
            });

        dbContext.AuditLogs.Add(new AuditLog
        {
            UserName = "system",
            Action = "Initialize",
            EntityName = "Database",
            Details = "SmartIT Pro v1.0 demo data created"
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
