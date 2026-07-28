using FluentValidation;
using MediatR;
using SmartIT.Domain;

namespace SmartIT.Application.Assets;

public interface IAssetRepository
{
    Task<IReadOnlyList<Asset>> ListAsync(CancellationToken cancellationToken);
    Task<Asset?> GetTrackedAsync(Guid id, CancellationToken cancellationToken);
    Task<Asset?> GetReadOnlyAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> AssetTagExistsAsync(string assetTag, Guid? excludingId, CancellationToken cancellationToken);
    Task<bool> SerialNumberExistsAsync(string serialNumber, Guid? excludingId, CancellationToken cancellationToken);
    Task AddAsync(Asset asset, CancellationToken cancellationToken);
}

public sealed record AssetListItem(
    Guid Id,
    string AssetTag,
    string Name,
    AssetType Type,
    AssetStatus Status,
    string? Manufacturer,
    string? Model,
    string? SerialNumber);

public sealed record GetAssetsQuery : IRequest<IReadOnlyList<AssetListItem>>;
public sealed record GetAssetByIdQuery(Guid Id) : IRequest<AssetListItem?>;
public sealed record CreateAssetCommand(
    string AssetTag,
    string Name,
    AssetType Type,
    AssetStatus Status,
    string? Manufacturer,
    string? Model,
    string? SerialNumber) : IRequest<AssetListItem>;
public sealed record UpdateAssetCommand(
    Guid Id,
    string AssetTag,
    string Name,
    AssetType Type,
    AssetStatus Status,
    string? Manufacturer,
    string? Model,
    string? SerialNumber) : IRequest<bool>;

public static class AssetMappings
{
    public static AssetListItem ToListItem(this Asset asset) => new(
        asset.Id, asset.AssetTag, asset.Name, asset.Type, asset.Status,
        asset.Manufacturer, asset.Model, asset.SerialNumber);
}

public sealed class GetAssetsHandler(IAssetRepository repository)
    : IRequestHandler<GetAssetsQuery, IReadOnlyList<AssetListItem>>
{
    public async Task<IReadOnlyList<AssetListItem>> Handle(GetAssetsQuery request, CancellationToken cancellationToken) =>
        (await repository.ListAsync(cancellationToken)).Select(asset => asset.ToListItem()).ToArray();
}

public sealed class GetAssetByIdHandler(IAssetRepository repository)
    : IRequestHandler<GetAssetByIdQuery, AssetListItem?>
{
    public async Task<AssetListItem?> Handle(GetAssetByIdQuery request, CancellationToken cancellationToken) =>
        (await repository.GetReadOnlyAsync(request.Id, cancellationToken))?.ToListItem();
}

public sealed class CreateAssetHandler(IAssetRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAssetCommand, AssetListItem>
{
    public async Task<AssetListItem> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = new Asset
        {
            AssetTag = request.AssetTag.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            Type = request.Type,
            Status = request.Status,
            Manufacturer = request.Manufacturer?.Trim(),
            Model = request.Model?.Trim(),
            SerialNumber = string.IsNullOrWhiteSpace(request.SerialNumber) ? null : request.SerialNumber.Trim()
        };

        await repository.AddAsync(asset, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return asset.ToListItem();
    }
}

public sealed class UpdateAssetHandler(IAssetRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAssetCommand, bool>
{
    public async Task<bool> Handle(UpdateAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await repository.GetTrackedAsync(request.Id, cancellationToken);
        if (asset is null) return false;

        asset.AssetTag = request.AssetTag.Trim().ToUpperInvariant();
        asset.Name = request.Name.Trim();
        asset.Type = request.Type;
        asset.Status = request.Status;
        asset.Manufacturer = request.Manufacturer?.Trim();
        asset.Model = request.Model?.Trim();
        asset.SerialNumber = string.IsNullOrWhiteSpace(request.SerialNumber) ? null : request.SerialNumber.Trim();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed class CreateAssetCommandValidator : AbstractValidator<CreateAssetCommand>
{
    public CreateAssetCommandValidator(IAssetRepository repository)
    {
        RuleFor(x => x.AssetTag).NotEmpty().MaximumLength(50).Matches("^[A-Za-z0-9._-]+$")
            .MustAsync(async (value, ct) => !await repository.AssetTagExistsAsync(value.Trim(), null, ct))
            .WithMessage("Asset tag already exists.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Manufacturer).MaximumLength(100);
        RuleFor(x => x.Model).MaximumLength(100);
        RuleFor(x => x.SerialNumber).MaximumLength(100)
            .MustAsync(async (value, ct) => string.IsNullOrWhiteSpace(value) || !await repository.SerialNumberExistsAsync(value.Trim(), null, ct))
            .WithMessage("Serial number already exists.");
    }
}

public sealed class UpdateAssetCommandValidator : AbstractValidator<UpdateAssetCommand>
{
    public UpdateAssetCommandValidator(IAssetRepository repository)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.AssetTag).NotEmpty().MaximumLength(50).Matches("^[A-Za-z0-9._-]+$")
            .MustAsync(async (command, value, ct) => !await repository.AssetTagExistsAsync(value.Trim(), command.Id, ct))
            .WithMessage("Asset tag already exists.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Manufacturer).MaximumLength(100);
        RuleFor(x => x.Model).MaximumLength(100);
        RuleFor(x => x.SerialNumber).MaximumLength(100)
            .MustAsync(async (command, value, ct) => string.IsNullOrWhiteSpace(value) || !await repository.SerialNumberExistsAsync(value.Trim(), command.Id, ct))
            .WithMessage("Serial number already exists.");
    }
}
