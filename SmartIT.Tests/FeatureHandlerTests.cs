using SmartIT.Application;
using SmartIT.Application.Assets;
using SmartIT.Application.Tickets;
using SmartIT.Domain;
using Xunit;

namespace SmartIT.Tests;

public sealed class FeatureHandlerTests
{
    [Fact]
    public async Task Create_asset_normalizes_input_and_commits_once()
    {
        var repository = new AssetRepositoryFake();
        var unitOfWork = new UnitOfWorkFake();
        var handler = new CreateAssetHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new CreateAssetCommand(" lap-100 ", " Developer Laptop ", AssetType.Laptop,
                AssetStatus.Available, " Dell ", " Latitude ", " SN-100 "),
            CancellationToken.None);

        Assert.Equal("LAP-100", result.AssetTag);
        Assert.Equal("Developer Laptop", result.Name);
        Assert.Equal(1, unitOfWork.SaveCount);
        Assert.Single(repository.Assets);
    }

    [Fact]
    public async Task Create_ticket_forces_open_status_and_generates_number()
    {
        var repository = new TicketRepositoryFake();
        var unitOfWork = new UnitOfWorkFake();
        var handler = new CreateTicketHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new CreateTicketCommand(" VPN issue ", " Cannot connect ", TicketPriority.High, null),
            CancellationToken.None);

        Assert.Equal(TicketStatus.Open, result.Status);
        Assert.StartsWith("INC-", result.Number);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    private sealed class UnitOfWorkFake : IUnitOfWork
    {
        public int SaveCount { get; private set; }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class AssetRepositoryFake : IAssetRepository
    {
        public List<Asset> Assets { get; } = [];
        public Task<IReadOnlyList<Asset>> ListAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Asset>>(Assets);
        public Task<Asset?> GetTrackedAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Assets.FirstOrDefault(x => x.Id == id));
        public Task<Asset?> GetReadOnlyAsync(Guid id, CancellationToken cancellationToken) => GetTrackedAsync(id, cancellationToken);
        public Task<Asset?> GetDetailsAsync(Guid id, CancellationToken cancellationToken) => GetTrackedAsync(id, cancellationToken);
        public Task<bool> AssetTagExistsAsync(string assetTag, Guid? excludingId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<bool> SerialNumberExistsAsync(string serialNumber, Guid? excludingId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task AddAsync(Asset asset, CancellationToken cancellationToken) { Assets.Add(asset); return Task.CompletedTask; }
    }

    private sealed class TicketRepositoryFake : ITicketRepository
    {
        public List<Ticket> Tickets { get; } = [];
        public Task<IReadOnlyList<Ticket>> ListAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Ticket>>(Tickets);
        public Task<Ticket?> GetTrackedAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Tickets.FirstOrDefault(x => x.Id == id));
        public Task<Ticket?> GetReadOnlyAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Tickets.FirstOrDefault(x => x.Id == id));
        public Task<bool> RequesterExistsAsync(Guid requesterId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<string> GenerateNextNumberAsync(CancellationToken cancellationToken) => Task.FromResult("INC-20260722-ABC12345");
        public Task AddAsync(Ticket ticket, CancellationToken cancellationToken) { Tickets.Add(ticket); return Task.CompletedTask; }
    }
}
