using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using CMS.Application.DTOs;
using CMS.Application.Features.Sites.Commands;
using CMS.Application.Features.Pages.Commands;
using CMS.Application.Features.Products.Commands;
using CMS.Application.Features.Destinations.Commands;

namespace CMS.API.IntegrationTests.Controllers;

public class ContentControllerIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;

    public ContentControllerIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateTestSiteAsync()
    {
        var command = new CreateSiteCommand
        {
            Name = $"Content Test Site {Guid.NewGuid()}",
            Domain = $"contenttest-{Guid.NewGuid()}.example.com"
        };
        var response = await _client.PostAsJsonAsync("/api/sites", command);
        var site = await response.Content.ReadFromJsonAsync<SiteDto>();
        return site!.Id;
    }

    private async Task<Guid> CreateTestPageAsync(Guid siteId)
    {
        var command = new CreatePageCommand
        {
            SiteId = siteId,
            PageId = $"test-page-{Guid.NewGuid()}",
            Title = "Test Page",
            Description = "Test Description",
            IsPublished = true
        };
        var response = await _client.PostAsJsonAsync($"/api/sites/{siteId}/pages", command);
        var page = await response.Content.ReadFromJsonAsync<PageDto>();
        return page!.Id;
    }

    private async Task<Guid> CreateTestProductAsync(Guid siteId)
    {
        var command = new CreateProductCommand
        {
            SiteId = siteId,
            ProductId = $"test-product-{Guid.NewGuid()}",
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            IsPublished = true
        };
        var response = await _client.PostAsJsonAsync($"/api/sites/{siteId}/products", command);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        return product!.Id;
    }

    private async Task<Guid> CreateTestDestinationAsync(Guid siteId)
    {
        var command = new CreateDestinationCommand
        {
            SiteId = siteId,
            DestinationId = $"test-destination-{Guid.NewGuid()}",
            Name = "Test Destination",
            Description = "Test Description",
            IsPublished = true
        };
        var response = await _client.PostAsJsonAsync($"/api/sites/{siteId}/destinations", command);
        var destination = await response.Content.ReadFromJsonAsync<DestinationDto>();
        return destination!.Id;
    }

    [Fact]
    public async Task GetSiteContent_ReturnsJson_WhenSiteExists()
    {
        // Arrange
        var siteId = await CreateTestSiteAsync();
        await CreateTestPageAsync(siteId);
        await CreateTestProductAsync(siteId);

        // Act
        var response = await _client.GetAsync($"/api/content/site/{siteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        
        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotBeNullOrEmpty();
        
        // Verify it's valid JSON
        var document = JsonDocument.Parse(json);
        document.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSiteContent_ReturnsJson_WhenSiteHasNoContent()
    {
        // Arrange
        var siteId = await CreateTestSiteAsync();

        // Act
        var response = await _client.GetAsync($"/api/content/site/{siteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        
        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExportSiteContent_ReturnsExport_WhenSiteExists()
    {
        // Arrange
        var siteId = await CreateTestSiteAsync();
        await CreateTestPageAsync(siteId);
        await CreateTestProductAsync(siteId);
        await CreateTestDestinationAsync(siteId);

        // Act
        var response = await _client.GetAsync($"/api/content/export/{siteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var export = await response.Content.ReadFromJsonAsync<SiteExportDto>();
        export.Should().NotBeNull();
        export!.SiteId.Should().Be(siteId);
        export.Name.Should().NotBeNullOrEmpty();
        export.Domain.Should().NotBeNullOrEmpty();
        export.ExportedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task ExportSiteContent_IncludesPages_WhenPagesExist()
    {
        // Arrange
        var siteId = await CreateTestSiteAsync();
        await CreateTestPageAsync(siteId);
        await CreateTestPageAsync(siteId);

        // Act
        var response = await _client.GetAsync($"/api/content/export/{siteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var export = await response.Content.ReadFromJsonAsync<SiteExportDto>();
        export.Should().NotBeNull();
        export!.Pages.Should().HaveCountGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task ExportSiteContent_IncludesProducts_WhenProductsExist()
    {
        // Arrange
        var siteId = await CreateTestSiteAsync();
        await CreateTestProductAsync(siteId);
        await CreateTestProductAsync(siteId);

        // Act
        var response = await _client.GetAsync($"/api/content/export/{siteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var export = await response.Content.ReadFromJsonAsync<SiteExportDto>();
        export.Should().NotBeNull();
        export!.Products.Should().HaveCountGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task ExportSiteContent_IncludesDestinations_WhenDestinationsExist()
    {
        // Arrange
        var siteId = await CreateTestSiteAsync();
        await CreateTestDestinationAsync(siteId);
        await CreateTestDestinationAsync(siteId);

        // Act
        var response = await _client.GetAsync($"/api/content/export/{siteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var export = await response.Content.ReadFromJsonAsync<SiteExportDto>();
        export.Should().NotBeNull();
        export!.Destinations.Should().HaveCountGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task ExportSiteContent_ReturnsNotFound_WhenSiteDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync($"/api/content/export/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPluginContent_ReturnsNotFound_WhenPluginDoesNotExist()
    {
        // Arrange
        var siteId = await CreateTestSiteAsync();

        // Act
        var response = await _client.GetAsync($"/api/content/site/{siteId}/plugin/NonExistentPlugin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSiteContent_HandlesMultipleSites_Independently()
    {
        // Arrange
        var siteId1 = await CreateTestSiteAsync();
        var siteId2 = await CreateTestSiteAsync();
        
        await CreateTestPageAsync(siteId1);
        await CreateTestProductAsync(siteId2);

        // Act
        var response1 = await _client.GetAsync($"/api/content/site/{siteId1}");
        var response2 = await _client.GetAsync($"/api/content/site/{siteId2}");

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var json1 = await response1.Content.ReadAsStringAsync();
        var json2 = await response2.Content.ReadAsStringAsync();
        
        // Both sites should return valid JSON (even if empty when no plugins are configured)
        json1.Should().NotBeNullOrEmpty();
        json2.Should().NotBeNullOrEmpty();
        
        // Verify both are valid JSON
        var doc1 = JsonDocument.Parse(json1);
        var doc2 = JsonDocument.Parse(json2);
        doc1.Should().NotBeNull();
        doc2.Should().NotBeNull();
    }

    [Fact]
    public async Task ExportSiteContent_ReturnsCompleteData_WhenSiteHasAllContentTypes()
    {
        // Arrange
        var siteId = await CreateTestSiteAsync();
        await CreateTestPageAsync(siteId);
        await CreateTestProductAsync(siteId);
        await CreateTestDestinationAsync(siteId);

        // Act
        var response = await _client.GetAsync($"/api/content/export/{siteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var export = await response.Content.ReadFromJsonAsync<SiteExportDto>();
        export.Should().NotBeNull();
        export!.SiteId.Should().Be(siteId);
        export.Pages.Should().NotBeEmpty();
        export.Products.Should().NotBeEmpty();
        export.Destinations.Should().NotBeEmpty();
    }
}
