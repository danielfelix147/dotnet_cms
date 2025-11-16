using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using CMS.Application.DTOs;
using CMS.Application.Features.Sites.Commands;

namespace CMS.API.IntegrationTests.Controllers;

public class SitesControllerIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;

    public SitesControllerIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllSites_ReturnsOkStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/sites");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var sites = await response.Content.ReadFromJsonAsync<IEnumerable<SiteDto>>();
        sites.Should().NotBeNull();
        // Note: We don't assert empty because tests run in parallel and may share database state
        // This test now verifies the endpoint works correctly
    }

    [Fact]
    public async Task CreateSite_ReturnsCreatedSite_WhenValidRequest()
    {
        // Arrange
        var command = new CreateSiteCommand
        {
            Name = "Test Site",
            Domain = "test.example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sites", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var site = await response.Content.ReadFromJsonAsync<SiteDto>();
        site.Should().NotBeNull();
        site!.Name.Should().Be("Test Site");
        site.Domain.Should().Be("test.example.com");
    }

    [Fact]
    public async Task GetSiteById_ReturnsSite_WhenSiteExists()
    {
        // Arrange
        var createCommand = new CreateSiteCommand
        {
            Name = "Get Test Site",
            Domain = "gettest.example.com"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/sites", createCommand);
        var createdSite = await createResponse.Content.ReadFromJsonAsync<SiteDto>();

        // Act
        var response = await _client.GetAsync($"/api/sites/{createdSite!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var site = await response.Content.ReadFromJsonAsync<SiteDto>();
        site.Should().NotBeNull();
        site!.Id.Should().Be(createdSite.Id);
        site.Name.Should().Be("Get Test Site");
    }

    [Fact]
    public async Task UpdateSite_ReturnsUpdatedSite_WhenValidRequest()
    {
        // Arrange
        var createCommand = new CreateSiteCommand
        {
            Name = "Update Test Site",
            Domain = "updatetest.example.com"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/sites", createCommand);
        var createdSite = await createResponse.Content.ReadFromJsonAsync<SiteDto>();

        var updateCommand = new UpdateSiteCommand
        {
            Id = createdSite!.Id,
            Name = "Updated Site Name",
            Domain = "updated.example.com"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sites/{createdSite.Id}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var site = await response.Content.ReadFromJsonAsync<SiteDto>();
        site.Should().NotBeNull();
        site!.Name.Should().Be("Updated Site Name");
        site.Domain.Should().Be("updated.example.com");
    }

    [Fact]
    public async Task DeleteSite_ReturnsNoContent_WhenSiteExists()
    {
        // Arrange
        var createCommand = new CreateSiteCommand
        {
            Name = "Delete Test Site",
            Domain = "deletetest.example.com"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/sites", createCommand);
        var createdSite = await createResponse.Content.ReadFromJsonAsync<SiteDto>();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/sites/{createdSite!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify soft delete - site should not be returned
        var getResponse = await _client.GetAsync($"/api/sites/{createdSite.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSiteById_ReturnsNotFound_WhenSiteDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync($"/api/sites/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
