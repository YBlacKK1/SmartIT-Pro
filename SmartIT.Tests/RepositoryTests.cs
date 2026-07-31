using Microsoft.EntityFrameworkCore;
using SmartIT.Domain;
using SmartIT.Infrastructure;
using Xunit;

namespace SmartIT.Tests;

public sealed class RepositoryTests
{
    [Fact]
    public async Task Adds_and_retrieves_asset()
    {
        var options = new DbContextOptionsBuilder<SmartITDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new SmartITDbContext(options);
        var repository = new Repository<Asset>(dbContext);
        var asset = new Asset
        {
            AssetTag = "TEST-001",
            Name = "Test Laptop",
            Type = AssetType.Laptop
        };

        await repository.AddAsync(asset);
        await dbContext.SaveChangesAsync();

        var result = await repository.GetByIdAsync(asset.Id);

        Assert.NotNull(result);
        Assert.Equal("TEST-001", result!.AssetTag);
    }
}
