using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartIT.Application;
using SmartIT.Application.Assets;
using SmartIT.Application.Employees;
using SmartIT.Application.Tickets;
using SmartIT.Domain;
using SmartIT.Web.Hubs;

namespace SmartIT.Web.Controllers;

public sealed record AssetDetailsViewModel(Asset Asset, IReadOnlyList<Employee> Employees);

[Authorize]
public sealed class EmployeesController(ISender sender, IWebHostEnvironment environment) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var employees = await sender.Send(new GetEmployeesQuery(), cancellationToken);
        return View(employees.Select(ToDto).ToArray());
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateDepartmentsAsync(cancellationToken);
        return View(new EmployeeDto(Guid.Empty, "", "", "", null, Guid.Empty, null, null));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeDto dto, IFormFile? photo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDepartmentsAsync(cancellationToken);
            return View(dto);
        }

        string? photoPath = null;
        try
        {
            photoPath = await SaveProfilePhotoAsync(photo, cancellationToken);
            await sender.Send(new CreateEmployeeCommand(
                dto.FirstName, dto.LastName, dto.Email, dto.JobTitle, dto.DepartmentId, photoPath), cancellationToken);
            TempData["Success"] = "Employee created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException exception)
        {
            AddValidationErrors(exception);
            await PopulateDepartmentsAsync(cancellationToken);
            return View(dto with { ProfilePhotoPath = photoPath });
        }
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var employee = await sender.Send(new GetEmployeeByIdQuery(id), cancellationToken);
        if (employee is null) return NotFound();

        await PopulateDepartmentsAsync(cancellationToken);
        return View(ToDto(employee));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EmployeeDto dto, IFormFile? photo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDepartmentsAsync(cancellationToken);
            return View(dto);
        }

        try
        {
            var newPhotoPath = await SaveProfilePhotoAsync(photo, cancellationToken);
            var updated = await sender.Send(new UpdateEmployeeCommand(
                dto.Id, dto.FirstName, dto.LastName, dto.Email, dto.JobTitle,
                dto.DepartmentId, newPhotoPath), cancellationToken);

            if (!updated) return NotFound();
            TempData["Success"] = "Employee information updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException exception)
        {
            AddValidationErrors(exception);
            await PopulateDepartmentsAsync(cancellationToken);
            return View(dto);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await sender.Send(new DeleteEmployeeCommand(id), cancellationToken);
        if (deleted)
        {
            TempData["Success"] = "Employee removed.";
        }
        else
        {
            TempData["Error"] = "Employee could not be removed because an asset assignment history is linked to this record.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDepartmentsAsync(CancellationToken cancellationToken) =>
        ViewBag.Departments = await sender.Send(new GetDepartmentsQuery(), cancellationToken);

    private async Task<string?> SaveProfilePhotoAsync(IFormFile? photo, CancellationToken cancellationToken)
    {
        if (photo is null || photo.Length == 0) return null;
        if (photo.Length > 5 * 1024 * 1024)
            throw new ValidationException("Profile photo cannot exceed 5 MB.");

        var allowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp"
        };
        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        if (!allowedContentTypes.Contains(photo.ContentType) || !allowedExtensions.Contains(extension))
            throw new ValidationException("Only JPG, PNG and WEBP images are allowed.");

        var uploads = Path.Combine(environment.WebRootPath, "uploads", "profiles");
        Directory.CreateDirectory(uploads);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploads, fileName);
        await using var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
        await photo.CopyToAsync(stream, cancellationToken);
        return $"/uploads/profiles/{fileName}";
    }

    private void AddValidationErrors(ValidationException exception)
    {
        if (exception.Errors.Any())
        {
            foreach (var error in exception.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
        else
        {
            ModelState.AddModelError(string.Empty, exception.Message);
        }
    }

    private static EmployeeDto ToDto(EmployeeListItem employee) => new(
        employee.Id, employee.FirstName, employee.LastName, employee.Email,
        employee.JobTitle, employee.DepartmentId, employee.DepartmentName, employee.ProfilePhotoPath);
}

[Authorize]
public sealed class AssetsController(
    ISender sender,
    IHubContext<NotificationHub> hub,
    IAssetRepository assetRepository,
    IEmployeeRepository employeeRepository) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var assets = await sender.Send(new GetAssetsQuery(), cancellationToken);
        return View(assets.Select(ToDto).ToArray());
    }

    public IActionResult Create() =>
        View(new AssetDto(Guid.Empty, "", "", AssetType.Laptop, AssetStatus.Available, null, null, null));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssetDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            var created = await sender.Send(new CreateAssetCommand(
                dto.AssetTag, dto.Name, dto.Type, dto.Status, dto.Manufacturer, dto.Model, dto.SerialNumber), cancellationToken);
            await hub.Clients.All.SendAsync("notification", $"Asset {dto.AssetTag} added", cancellationToken);
            TempData["Success"] = "Asset added to inventory.";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (ValidationException exception)
        {
            AddValidationErrors(exception);
            return View(dto);
        }
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var asset = await assetRepository.GetDetailsAsync(id, cancellationToken);
        if (asset is null) return NotFound();

        var employees = await employeeRepository.ListAsync(cancellationToken);
        return View(new AssetDetailsViewModel(asset, employees));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var asset = await sender.Send(new GetAssetByIdQuery(id), cancellationToken);
        return asset is null ? NotFound() : View(ToDto(asset));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AssetDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            var updated = await sender.Send(new UpdateAssetCommand(
                dto.Id, dto.AssetTag, dto.Name, dto.Type, dto.Status,
                dto.Manufacturer, dto.Model, dto.SerialNumber), cancellationToken);

            if (!updated) return NotFound();
            TempData["Success"] = "Asset information updated.";
            return RedirectToAction(nameof(Details), new { id = dto.Id });
        }
        catch (ValidationException exception)
        {
            AddValidationErrors(exception);
            return View(dto);
        }
    }

    private void AddValidationErrors(ValidationException exception)
    {
        if (exception.Errors.Any())
        {
            foreach (var error in exception.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
        else
        {
            ModelState.AddModelError(string.Empty, exception.Message);
        }
    }

    private static AssetDto ToDto(AssetListItem asset) => new(
        asset.Id, asset.AssetTag, asset.Name, asset.Type, asset.Status,
        asset.Manufacturer, asset.Model, asset.SerialNumber);
}

[Authorize]
public sealed class TicketsController(
    ISender sender,
    IHubContext<NotificationHub> hub,
    IEmployeeRepository employeeRepository,
    IRepository<AuditLog> auditLogs,
    IUnitOfWork unitOfWork) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var tickets = await sender.Send(new GetTicketsQuery(), cancellationToken);
        return View(tickets.Select(ToDto).ToArray());
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateEmployeesAsync(cancellationToken);
        return View(new TicketDto(
            Guid.Empty, "", "", "", TicketPriority.Medium, TicketStatus.Open, null, null, DateTime.UtcNow));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TicketDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateEmployeesAsync(cancellationToken);
            return View(dto);
        }

        try
        {
            var ticket = await sender.Send(
                new CreateTicketCommand(dto.Subject, dto.Description, dto.Priority, dto.RequesterId), cancellationToken);
            await hub.Clients.All.SendAsync("notification", $"New ticket: {dto.Subject}", cancellationToken);
            TempData["Success"] = "Help desk ticket created.";
            return RedirectToAction(nameof(Details), new { id = ticket.Id });
        }
        catch (ValidationException exception)
        {
            foreach (var error in exception.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            await PopulateEmployeesAsync(cancellationToken);
            return View(dto);
        }
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await sender.Send(new GetTicketByIdQuery(id), cancellationToken);
        return ticket is null ? NotFound() : View(ToDto(ticket));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, TicketStatus status, CancellationToken cancellationToken)
    {
        var updated = await sender.Send(new UpdateTicketStatusCommand(id, status), cancellationToken);
        if (!updated) return NotFound();

        await auditLogs.AddAsync(new AuditLog
        {
            UserName = User.Identity?.Name ?? "system",
            Action = "Status change",
            EntityName = nameof(Ticket),
            Details = $"Ticket {id} changed to {status}",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await hub.Clients.All.SendAsync("notification", $"Ticket status changed to {status}", cancellationToken);
        TempData["Success"] = "Ticket status updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task PopulateEmployeesAsync(CancellationToken cancellationToken) =>
        ViewBag.Employees = await employeeRepository.ListAsync(cancellationToken);

    private static TicketDto ToDto(TicketListItem ticket) => new(
        ticket.Id, ticket.Number, ticket.Subject, ticket.Description, ticket.Priority,
        ticket.Status, ticket.RequesterId, ticket.RequesterName, ticket.CreatedAt);
}
