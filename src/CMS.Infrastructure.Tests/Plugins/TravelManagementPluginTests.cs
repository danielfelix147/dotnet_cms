using CMS.Domain.Entities;
using CMS.Infrastructure.Plugins;
using FluentAssertions;
using System.Text.Json;

namespace CMS.Infrastructure.Tests.Plugins;

public class TravelManagementPluginTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public TravelManagementPluginTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Plugin_Properties_Should_Be_Correct()
    {
        // Arrange
        var context = _fixture.CreateDbContext();
        var plugin = new TravelManagementPlugin(context);

        // Assert
        plugin.SystemName.Should().Be("TravelManagement");
        plugin.DisplayName.Should().Be("Travel Management");
        plugin.Description.Should().Be("Manage destinations and tours");
        plugin.Version.Should().Be("1.0.0");
    }

    [Fact]
    public async Task GetContentAsync_Should_Return_Published_Destinations()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var plugin = new TravelManagementPlugin(context);

        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Test Site",
            Domain = $"travel-plugin-{Guid.NewGuid()}.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Sites.Add(site);
        await context.SaveChangesAsync();

        var destination = new Destination
        {
            Id = Guid.NewGuid(),
            SiteId = site.Id,
            DestinationId = "paris",
            Name = "Paris, France",
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Destinations.Add(destination);
        await context.SaveChangesAsync();

        // Act
        var result = await plugin.GetContentAsync(site.Id);

        // Assert
        result.Should().NotBeNull();
        var resultList = result as List<object>;
        resultList.Should().NotBeNull();
        resultList!.Should().HaveCountGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task GetContentAsync_Should_Include_Tours()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var plugin = new TravelManagementPlugin(context);

        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Test Site",
            Domain = $"travel-tours-{Guid.NewGuid()}.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Sites.Add(site);
        await context.SaveChangesAsync();

        var destination = new Destination
        {
            Id = Guid.NewGuid(),
            SiteId = site.Id,
            DestinationId = "london",
            Name = "London, UK",
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Destinations.Add(destination);
        await context.SaveChangesAsync();

        var tour = new Tour
        {
            Id = Guid.NewGuid(),
            DestinationId = destination.Id,
            TourId = "london-highlights",
            Name = "London Highlights Tour",
            Price = 150.00m,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Tours.Add(tour);
        await context.SaveChangesAsync();

        // Act
        var result = await plugin.GetContentAsync(site.Id);

        // Assert
        result.Should().NotBeNull();
        var json = JsonSerializer.Serialize(result);
        json.Should().Contain("London Highlights Tour");
    }

    [Fact]
    public async Task GetContentAsync_Should_Include_Destination_And_Tour_Images()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var plugin = new TravelManagementPlugin(context);

        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Test Site",
            Domain = $"travel-media-{Guid.NewGuid()}.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Sites.Add(site);
        await context.SaveChangesAsync();

        var destination = new Destination
        {
            Id = Guid.NewGuid(),
            SiteId = site.Id,
            DestinationId = "rome",
            Name = "Rome, Italy",
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Destinations.Add(destination);
        await context.SaveChangesAsync();

        var destImage = new Image
        {
            Id = Guid.NewGuid(),
            EntityId = destination.Id,
            EntityType = "Destination",
            ImageId = "dest-img",
            Location = "/images/rome.jpg",
            CreatedAt = DateTime.UtcNow
        };
        context.Images.Add(destImage);
        await context.SaveChangesAsync();

        var tour = new Tour
        {
            Id = Guid.NewGuid(),
            DestinationId = destination.Id,
            TourId = "colosseum-tour",
            Name = "Colosseum Tour",
            Price = 75.00m,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Tours.Add(tour);
        await context.SaveChangesAsync();

        var tourImage = new Image
        {
            Id = Guid.NewGuid(),
            EntityId = tour.Id,
            EntityType = "Tour",
            ImageId = "tour-img",
            Location = "/images/colosseum.jpg",
            CreatedAt = DateTime.UtcNow
        };
        context.Images.Add(tourImage);
        await context.SaveChangesAsync();

        // Act
        var result = await plugin.GetContentAsync(site.Id);

        // Assert
        result.Should().NotBeNull();
        var json = JsonSerializer.Serialize(result);
        json.Should().Contain("dest-img");
        json.Should().Contain("tour-img");
    }

    [Fact]
    public async Task GenerateJsonAsync_Should_Return_Valid_Json()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var plugin = new TravelManagementPlugin(context);

        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Test Site",
            Domain = $"travel-json-{Guid.NewGuid()}.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Sites.Add(site);
        await context.SaveChangesAsync();

        // Act
        var result = await plugin.GenerateJsonAsync(site.Id);

        // Assert
        result.Should().NotBeNullOrEmpty();
        var action = () => JsonDocument.Parse(result);
        action.Should().NotThrow();
    }

    [Fact]
    public async Task GetContentAsync_Should_Not_Return_Unpublished_Tours()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var plugin = new TravelManagementPlugin(context);

        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Test Site",
            Domain = $"travel-unpub-{Guid.NewGuid()}.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Sites.Add(site);
        await context.SaveChangesAsync();

        var destination = new Destination
        {
            Id = Guid.NewGuid(),
            SiteId = site.Id,
            DestinationId = "tokyo",
            Name = "Tokyo, Japan",
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Destinations.Add(destination);
        await context.SaveChangesAsync();

        var unpublishedTour = new Tour
        {
            Id = Guid.NewGuid(),
            DestinationId = destination.Id,
            TourId = "unpub-tour",
            Name = "Unpublished Tour",
            Price = 100.00m,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow
        };
        context.Tours.Add(unpublishedTour);
        await context.SaveChangesAsync();

        // Act
        var result = await plugin.GetContentAsync(site.Id);

        // Assert
        result.Should().NotBeNull();
        var json = JsonSerializer.Serialize(result);
        json.Should().NotContain("Unpublished Tour");
    }
}
