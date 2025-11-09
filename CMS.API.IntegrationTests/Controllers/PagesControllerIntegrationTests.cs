using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using CMS.Application.DTOs;
using CMS.Application.Features.Sites.Commands;
using CMS.Application.Features.Pages.Commands;

namespace CMS.API.IntegrationTests.Controllers;

public class PagesControllerIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;

    public PagesControllerIntegrationTests(IntegrationTestWebAppFactory factory)
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
    public async Task GetAllPages_ReturnsEmptyList_WhenNoPagesExist()
    {
        // Arrange
        var site = await CreateTestSite();

        // Act
        var response = await _client.GetAsync($"/api/sites/{site.Id}/pages");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pages = await response.Content.ReadFromJsonAsync<IEnumerable<PageDto>>();
        pages.Should().NotBeNull();
        pages.Should().BeEmpty();
    }

    [Fact]
    public async Task CreatePage_ReturnsCreatedPage_WhenValidRequest()
    {
        // Arrange
        var site = await CreateTestSite();
        var command = new CreatePageCommand
        {
            SiteId = site.Id,
            PageId = "home",
            Title = "Home Page",
            IsPublished = true
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/sites/{site.Id}/pages", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var page = await response.Content.ReadFromJsonAsync<PageDto>();
        page.Should().NotBeNull();
        page!.PageId.Should().Be("home");
        page.Title.Should().Be("Home Page");
        page.IsPublished.Should().BeTrue();
    }

    [Fact]
    public async Task GetPageById_ReturnsPage_WhenPageExists()
    {
        // Arrange
        var site = await CreateTestSite();
        var createCommand = new CreatePageCommand
        {
            SiteId = site.Id,
            PageId = "about",
            Title = "About Page",
            IsPublished = true
        };
        var createResponse = await _client.PostAsJsonAsync($"/api/sites/{site.Id}/pages", createCommand);
        var createdPage = await createResponse.Content.ReadFromJsonAsync<PageDto>();

        // Act
        var response = await _client.GetAsync($"/api/sites/{site.Id}/pages/{createdPage!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PageDto>();
        page.Should().NotBeNull();
        page!.PageId.Should().Be("about");
    }

    [Fact]
    public async Task UpdatePage_ReturnsUpdatedPage_WhenValidRequest()
    {
        // Arrange
        var site = await CreateTestSite();
        var createCommand = new CreatePageCommand
        {
            SiteId = site.Id,
            PageId = "services",
            Title = "Services",
            IsPublished = true
        };
        var createResponse = await _client.PostAsJsonAsync($"/api/sites/{site.Id}/pages", createCommand);
        var createdPage = await createResponse.Content.ReadFromJsonAsync<PageDto>();

        var updateCommand = new UpdatePageCommand
        {
            Id = createdPage!.Id,
            SiteId = site.Id,
            PageId = "services-updated",
            Title = "Our Services",
            IsPublished = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sites/{site.Id}/pages/{createdPage.Id}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PageDto>();
        page.Should().NotBeNull();
        page!.PageId.Should().Be("services-updated");
        page.Title.Should().Be("Our Services");
        page.IsPublished.Should().BeFalse();
    }

    [Fact]
    public async Task DeletePage_ReturnsNoContent_WhenPageExists()
    {
        // Arrange
        var site = await CreateTestSite();
        var createCommand = new CreatePageCommand
        {
            SiteId = site.Id,
            PageId = "contact",
            Title = "Contact",
            IsPublished = true
        };
        var createResponse = await _client.PostAsJsonAsync($"/api/sites/{site.Id}/pages", createCommand);
        var createdPage = await createResponse.Content.ReadFromJsonAsync<PageDto>();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/sites/{site.Id}/pages/{createdPage!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify soft delete
        var getResponse = await _client.GetAsync($"/api/sites/{site.Id}/pages/{createdPage.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
