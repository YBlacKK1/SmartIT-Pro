using AutoMapper;
using FluentValidation;
using SmartIT.Domain;

namespace SmartIT.Application;

public interface IRepository<T> where T : Entity
{
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed record EmployeeDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? JobTitle,
    Guid DepartmentId,
    string? DepartmentName,
    string? ProfilePhotoPath);

public sealed record AssetDto(
    Guid Id,
    string AssetTag,
    string Name,
    AssetType Type,
    AssetStatus Status,
    string? Manufacturer,
    string? Model,
    string? SerialNumber);

public sealed record TicketDto(
    Guid Id,
    string Number,
    string Subject,
    string Description,
    TicketPriority Priority,
    TicketStatus Status,
    Guid? RequesterId,
    DateTime CreatedAt);

public sealed record DashboardDto(
    int Employees,
    int Assets,
    int OpenTickets,
    int AssignedAssets,
    IReadOnlyCollection<TicketDto> RecentTickets,
    IReadOnlyDictionary<string, int> AssetStatus);

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Employee, EmployeeDto>()
            .ForCtorParam(nameof(EmployeeDto.DepartmentName), options =>
                options.MapFrom(source => source.Department == null ? null : source.Department.Name));

        CreateMap<EmployeeDto, Employee>()
            .ForMember(destination => destination.Department, options => options.Ignore())
            .ForMember(destination => destination.Assignments, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore());

        CreateMap<Asset, AssetDto>();
        CreateMap<AssetDto, Asset>()
            .ForMember(destination => destination.Assignments, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore());

        CreateMap<Ticket, TicketDto>();
        CreateMap<TicketDto, Ticket>()
            .ForMember(destination => destination.Requester, options => options.Ignore())
            .ForMember(destination => destination.Comments, options => options.Ignore())
            .ForMember(destination => destination.Attachments, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore());
    }
}

public sealed class EmployeeValidator : AbstractValidator<EmployeeDto>
{
    public EmployeeValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.JobTitle).MaximumLength(150);
        RuleFor(x => x.DepartmentId).NotEmpty();
    }
}

public sealed class AssetValidator : AbstractValidator<AssetDto>
{
    public AssetValidator()
    {
        RuleFor(x => x.AssetTag).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Manufacturer).MaximumLength(100);
        RuleFor(x => x.Model).MaximumLength(100);
        RuleFor(x => x.SerialNumber).MaximumLength(100);
    }
}

public sealed class TicketValidator : AbstractValidator<TicketDto>
{
    public TicketValidator()
    {
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
    }
}

public interface IDashboardService
{
    Task<DashboardDto> GetAsync(CancellationToken cancellationToken = default);
}
