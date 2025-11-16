using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using CMS.Application.DTOs;
using CMS.Application.Features.Sites.Commands;
using CMS.Application.Features.Destinations.Commands;
using CMS.Application.Features.Tours.Commands;

namespace CMS.API.IntegrationTests.Controllers;

public class DestinationsControllerIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;

    public DestinationsControllerIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<SiteDto> CreateTestSite()
    {
        var command = new CreateSiteCommand
        {
            Name = "Test Site",
            Domain = $"test-{Guid.NewGuid()}.example.com"
        };
        var response = await _client.PostAsJsonAsync("/api/sites", command);
        return (await response.Content.ReadFromJsonAsync<SiteDto>())!;
    }

    [Fact]
    public async Task GetAllDestinations_ReturnsEmptyList_WhenNoDestinationsExist()
    {
        // Arrange
        var site = await CreateTestSite();

        // Act
        var response = await _client.GetAsync($"/api/sites/{site.Id}/destinations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var destinations = await response.Content.ReadFromJsonAsync<IEnumerable<DestinationDto>>();
        destinations.Should().NotBeNull();
        destinations.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateDestination_ReturnsCreatedDestination_WhenValidRequest()
    {
        // Arrange
        var site = await CreateTestSite();
        var command = new CreateDestinationCommand
        {
            SiteId = site.Id,
            DestinationId = "paris",
            Name = "Paris",
            Description = "The City of Light",
            IsPublished = true
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/sites/{site.Id}/destinations", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var destination = await response.Content.ReadFromJsonAsync<DestinationDto>();
        destination.Should().NotBeNull();
        destination!.DestinationId.Should().Be("paris");
        destination.Destination.Should().Be("Paris");
    }

    [Fact]
    public async Task GetDestinationById_ReturnsDestination_WhenDestinationExists()
    {
        // Arrange
        var site = await CreateTestSite();
        var createCommand = new CreateDestinationCommand
        {
            SiteId = site.Id,
            DestinationId = "london",
            Name = "London",
            IsPublished = true
        };
        var createResponse = await _client.PostAsJsonAsync($"/api/sites/{site.Id}/destinations", createCommand);
        var createdDestination = await createResponse.Content.ReadFromJsonAsync<DestinationDto>();

        // Act
        var response = await _client.GetAsync($"/api/sites/{site.Id}/destinations/{createdDestination!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var destination = await response.Content.ReadFromJsonAsync<DestinationDto>();
        destination.Should().NotBeNull();
        destination!.DestinationId.Should().Be("london");
    }

    [Fact]
    public async Task UpdateDestination_ReturnsUpdatedDestination_WhenValidRequest()
    {
        // Arrange
        var site = await CreateTestSite();
        var createCommand = new CreateDestinationCommand
        {
            SiteId = site.Id,
            DestinationId = "rome",
            Name = "Rome",
            IsPublished = true
        };
        var createResponse = await _client.PostAsJsonAsync($"/api/sites/{site.Id}/destinations", createCommand);
        var createdDestination = await createResponse.Content.ReadFromJsonAsync<DestinationDto>();

        var updateCommand = new UpdateDestinationCommand
        {
            Id = createdDestination!.Id,
            SiteId = site.Id,
            DestinationId = "rome-italy",
            Name = "Rome, Italy",
            Description = "The Eternal City",
            IsPublished = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sites/{site.Id}/destinations/{createdDestination.Id}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var destination = await response.Content.ReadFromJsonAsync<DestinationDto>();
        destination.Should().NotBeNull();
        destination!.DestinationId.Should().Be("rome-italy");
        destination.Destination.Should().Be("Rome, Italy");
    }

    [Fact]
    public async Task DeleteDestination_ReturnsNoContent_WhenDestinationExists()
    {
        // Arrange
        var site = await CreateTestSite();
        var createCommand = new CreateDestinationCommand
        {
            SiteId = site.Id,
            DestinationId = "barcelona",
            Name = "Barcelona",
            IsPublished = true
        };
        var createResponse = await _client.PostAsJsonAsync($"/api/sites/{site.Id}/destinations", createCommand);
        var createdDestination = await createResponse.Content.ReadFromJsonAsync<DestinationDto>();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/sites/{site.Id}/destinations/{createdDestination!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify soft delete
        var getResponse = await _client.GetAsync($"/api/sites/{site.Id}/destinations/{createdDestination.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTour_ReturnsCreatedTour_WhenValidRequest()
    {
        // Arrange
        var site = await CreateTestSite();
        var destCommand = new CreateDestinationCommand
        {
            SiteId = site.Id,
            DestinationId = "madrid",
            Name = "Madrid",
            IsPublished = true
        };
        var destResponse = await _client.PostAsJsonAsync($"/api/sites/{site.Id}/destinations", destCommand);
        var destination = await destResponse.Content.ReadFromJsonAsync<DestinationDto>();

        var tourCommand = new CreateTourCommand
        {
            DestinationId = destination!.Id,
            TourId = "madrid-city-tour",
            Name = "Madrid City Tour",
            Description = "Explore the heart of Madrid",
            Price = 89.99m,
            IsPublished = true
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/sites/{site.Id}/destinations/{destination.Id}/tours", tourCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var tour = await response.Content.ReadFromJsonAsync<TourDto>();
        tour.Should().NotBeNull();
        tour!.TourId.Should().Be("madrid-city-tour");
        tour.Name.Should().Be("Madrid City Tour");
        tour.Price.Should().Be(89.99m);
    }

    [Fact]
    public async Task GetTours_ReturnsTours_WhenToursExist()
    {
        // Arrange
        var site = await CreateTestSite();
        var destCommand = new CreateDestinationCommand
        {
            SiteId = site.Id,
            DestinationId = "berlin",
            Name = "Berlin",
            IsPublished = true
        };
        var destResponse = await _client.PostAsJsonAsync($"/api/sites/{site.Id}/destinations", destCommand);
        var destination = await destResponse.Content.ReadFromJsonAsync<DestinationDto>();

        var tourCommand = new CreateTourCommand
        {
            DestinationId = destination!.Id,
            TourId = "berlin-wall-tour",
            Name = "Berlin Wall Tour",
            Price = 59.99m,
            IsPublished = true
        };
        await _client.PostAsJsonAsync($"/api/sites/{site.Id}/destinations/{destination.Id}/tours", tourCommand);

        // Act
        var response = await _client.GetAsync($"/api/sites/{site.Id}/destinations/{destination.Id}/tours");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tours = await response.Content.ReadFromJsonAsync<IEnumerable<TourDto>>();
        tours.Should().NotBeNull();
        tours.Should().HaveCount(1);
        tours!.First().TourId.Should().Be("berlin-wall-tour");
    }

    [Fact]
    public async Task UpdateTour_ReturnsUpdatedTour_WhenValidRequest()
    {
        // Arrange
        var site = await CreateTestSite();
        var destCommand = new CreateDestinationCommand
        {
            SiteId = site.Id,
            DestinationId = "amsterdam",
            Name = "Amsterdam",
            IsPublished = true
        };
        var destResponse = await _client.PostAsJsonAsync($"/api/sites/{site.Id}/destinations", destCommand);
        var destination = await destResponse.Content.ReadFromJsonAsync<DestinationDto>();

        var createTourCommand = new CreateTourCommand
        {
            DestinationId = destination!.Id,
            TourId = "canal-tour",
            Name = "Canal Tour",
            Price = 45.00m,
            IsPublished = true
        };
        var createTourResponse = await _client.PostAsJsonAsync($"/api/sites/{site.Id}/destinations/{destination.Id}/tours", createTourCommand);
        var createdTour = await createTourResponse.Content.ReadFromJsonAsync<TourDto>();

        var updateTourCommand = new UpdateTourCommand
        {
            Id = createdTour!.Id,
            DestinationId = destination.Id,
            TourId = "canal-cruise",
            Name = "Amsterdam Canal Cruise",
            Description = "Scenic boat tour through Amsterdam canals",
            Price = 55.00m,
            IsPublished = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sites/{site.Id}/destinations/{destination.Id}/tours/{createdTour.Id}", updateTourCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tour = await response.Content.ReadFromJsonAsync<TourDto>();
        tour.Should().NotBeNull();
        tour!.TourId.Should().Be("canal-cruise");
        tour.Name.Should().Be("Amsterdam Canal Cruise");
        tour.Price.Should().Be(55.00m);
    }

    [Fact]
    public async Task DeleteTour_ReturnsNoContent_WhenTourExists()
    {
        // Arrange
        var site = await CreateTestSite();
        var destCommand = new CreateDestinationCommand
        {
            SiteId = site.Id,
            DestinationId = "vienna",
            Name = "Vienna",
            IsPublished = true
        };
        var destResponse = await _client.PostAsJsonAsync($"/api/sites/{site.Id}/destinations", destCommand);
        var destination = await destResponse.Content.ReadFromJsonAsync<DestinationDto>();

        var tourCommand = new CreateTourCommand
        {
            DestinationId = destination!.Id,
            TourId = "palace-tour",
            Name = "Palace Tour",
            Price = 79.99m,
            IsPublished = true
        };
        var tourResponse = await _client.PostAsJsonAsync($"/api/sites/{site.Id}/destinations/{destination.Id}/tours", tourCommand);
        var tour = await tourResponse.Content.ReadFromJsonAsync<TourDto>();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/sites/{site.Id}/destinations/{destination.Id}/tours/{tour!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
