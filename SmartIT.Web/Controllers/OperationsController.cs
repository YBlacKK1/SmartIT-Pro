using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartIT.Application;
using SmartIT.Domain;
using SmartIT.Infrastructure;

namespace SmartIT.Web.Controllers;

[Authorize(Roles = "Admin")]
public sealed class OperationsController(
    IRepository<Asset> assets,
    IRepository<Employee> employees,
    IRepository<AssetAssignment> assignments,
    IRepository<SoftwareLicense> licenses,
    IRepository<AuditLog> audit,
    IUnitOfWork unitOfWork,
    SmartITDbContext dbContext) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(
        Guid assetId,
        Guid employeeId,
        string? notes,
        CancellationToken cancellationToken)
    {
        var asset = await assets.GetByIdAsync(assetId, cancellationToken);
        var employee = await employees.GetByIdAsync(employeeId, cancellationToken);

        if (asset is null || employee is null || asset.Status != AssetStatus.Available)
        {
            TempData["Error"] = "The asset cannot be assigned.";
            return RedirectToAction("Index", "Assets");
        }

        asset.Status = AssetStatus.Assigned;
        await assignments.AddAsync(new AssetAssignment
        {
            AssetId = assetId,
            EmployeeId = employeeId,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        }, cancellationToken);

        await audit.AddAsync(new AuditLog
        {
            UserName = User.Identity?.Name ?? "system",
            Action = "Assign",
            EntityName = nameof(Asset),
            Details = $"{asset.AssetTag} assigned to {employee.FullName}",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        TempData["Success"] = $"{asset.AssetTag} assigned to {employee.FullName}.";
        return RedirectToAction("Details", "Assets", new { id = assetId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        var assignment = await assignments.GetByIdAsync(assignmentId, cancellationToken);
        if (assignment is null || assignment.ReturnedAt is not null)
        {
            return NotFound();
        }

        assignment.ReturnedAt = DateTime.UtcNow;
        var asset = await assets.GetByIdAsync(assignment.AssetId, cancellationToken);
        if (asset is not null)
        {
            asset.Status = AssetStatus.Available;
        }

        await audit.AddAsync(new AuditLog
        {
            UserName = User.Identity?.Name ?? "system",
            Action = "Return",
            EntityName = nameof(Asset),
            Details = asset is null ? $"Assignment {assignmentId} returned" : $"{asset.AssetTag} returned to inventory",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Asset returned to inventory.";
        return RedirectToAction("Details", "Assets", new { id = assignment.AssetId });
    }

    public async Task<IActionResult> Licenses(CancellationToken cancellationToken) =>
        View(await licenses.GetAllAsync(cancellationToken));

    public async Task<IActionResult> Maintenance(CancellationToken cancellationToken) =>
        View(await dbContext.MaintenanceSchedules
            .AsNoTracking()
            .Include(x => x.Asset)
            .OrderBy(x => x.ScheduledFor)
            .ToListAsync(cancellationToken));

    public async Task<IActionResult> Audit(CancellationToken cancellationToken) =>
        View(await audit.GetAllAsync(cancellationToken));
}
