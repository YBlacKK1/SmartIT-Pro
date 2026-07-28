using FluentValidation;
using MediatR;
using SmartIT.Domain;

namespace SmartIT.Application.Employees;

public interface IEmployeeRepository
{
    Task<IReadOnlyList<Employee>> ListAsync(CancellationToken cancellationToken);
    Task<Employee?> GetTrackedAsync(Guid id, CancellationToken cancellationToken);
    Task<Employee?> GetReadOnlyAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, Guid? excludingId, CancellationToken cancellationToken);
    Task<bool> DepartmentExistsAsync(Guid departmentId, CancellationToken cancellationToken);
    Task AddAsync(Employee employee, CancellationToken cancellationToken);
    void Remove(Employee employee);
}

public interface IDepartmentRepository
{
    Task<IReadOnlyList<Department>> ListAsync(CancellationToken cancellationToken);
}

public sealed record EmployeeListItem(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? JobTitle,
    Guid DepartmentId,
    string? DepartmentName,
    string? ProfilePhotoPath);

public sealed record GetEmployeesQuery : IRequest<IReadOnlyList<EmployeeListItem>>;
public sealed record GetEmployeeByIdQuery(Guid Id) : IRequest<EmployeeListItem?>;
public sealed record GetDepartmentsQuery : IRequest<IReadOnlyList<Department>>;
public sealed record CreateEmployeeCommand(
    string FirstName,
    string LastName,
    string Email,
    string? JobTitle,
    Guid DepartmentId,
    string? ProfilePhotoPath) : IRequest<EmployeeListItem>;
public sealed record UpdateEmployeeCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? JobTitle,
    Guid DepartmentId,
    string? ProfilePhotoPath) : IRequest<bool>;
public sealed record DeleteEmployeeCommand(Guid Id) : IRequest<bool>;

public static class EmployeeMappings
{
    public static EmployeeListItem ToListItem(this Employee employee) => new(
        employee.Id,
        employee.FirstName,
        employee.LastName,
        employee.Email,
        employee.JobTitle,
        employee.DepartmentId,
        employee.Department?.Name,
        employee.ProfilePhotoPath);
}

public sealed class GetEmployeesHandler(IEmployeeRepository repository)
    : IRequestHandler<GetEmployeesQuery, IReadOnlyList<EmployeeListItem>>
{
    public async Task<IReadOnlyList<EmployeeListItem>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken) =>
        (await repository.ListAsync(cancellationToken)).Select(employee => employee.ToListItem()).ToArray();
}

public sealed class GetEmployeeByIdHandler(IEmployeeRepository repository)
    : IRequestHandler<GetEmployeeByIdQuery, EmployeeListItem?>
{
    public async Task<EmployeeListItem?> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken) =>
        (await repository.GetReadOnlyAsync(request.Id, cancellationToken))?.ToListItem();
}

public sealed class GetDepartmentsHandler(IDepartmentRepository repository)
    : IRequestHandler<GetDepartmentsQuery, IReadOnlyList<Department>>
{
    public Task<IReadOnlyList<Department>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken) =>
        repository.ListAsync(cancellationToken);
}

public sealed class CreateEmployeeHandler(IEmployeeRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateEmployeeCommand, EmployeeListItem>
{
    public async Task<EmployeeListItem> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = new Employee
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            JobTitle = request.JobTitle?.Trim(),
            DepartmentId = request.DepartmentId,
            ProfilePhotoPath = request.ProfilePhotoPath
        };

        await repository.AddAsync(employee, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return employee.ToListItem();
    }
}

public sealed class UpdateEmployeeHandler(IEmployeeRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateEmployeeCommand, bool>
{
    public async Task<bool> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await repository.GetTrackedAsync(request.Id, cancellationToken);
        if (employee is null) return false;

        employee.FirstName = request.FirstName.Trim();
        employee.LastName = request.LastName.Trim();
        employee.Email = request.Email.Trim().ToLowerInvariant();
        employee.JobTitle = request.JobTitle?.Trim();
        employee.DepartmentId = request.DepartmentId;
        if (request.ProfilePhotoPath is not null)
        {
            employee.ProfilePhotoPath = request.ProfilePhotoPath;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed class DeleteEmployeeHandler(IEmployeeRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteEmployeeCommand, bool>
{
    public async Task<bool> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await repository.GetTrackedAsync(request.Id, cancellationToken);
        if (employee is null) return false;

        repository.Remove(employee);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator(IEmployeeRepository repository)
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256)
            .MustAsync(async (email, ct) => !await repository.EmailExistsAsync(email.Trim(), null, ct))
            .WithMessage("Email address already exists.");
        RuleFor(x => x.JobTitle).MaximumLength(150);
        RuleFor(x => x.DepartmentId).NotEmpty()
            .MustAsync(repository.DepartmentExistsAsync)
            .WithMessage("Department does not exist.");
        RuleFor(x => x.ProfilePhotoPath).MaximumLength(500);
    }
}

public sealed class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator(IEmployeeRepository repository)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256)
            .MustAsync(async (command, email, ct) => !await repository.EmailExistsAsync(email.Trim(), command.Id, ct))
            .WithMessage("Email address already exists.");
        RuleFor(x => x.JobTitle).MaximumLength(150);
        RuleFor(x => x.DepartmentId).NotEmpty()
            .MustAsync(repository.DepartmentExistsAsync)
            .WithMessage("Department does not exist.");
        RuleFor(x => x.ProfilePhotoPath).MaximumLength(500);
    }
}
