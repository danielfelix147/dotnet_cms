using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using CMS.Application.DTOs;
using CMS.Application.Features.Sites.Commands;
using CMS.Application.Features.Products.Commands;

namespace CMS.API.IntegrationTests.Controllers;

public class ProductsControllerIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;

    public ProductsControllerIntegrationTests(IntegrationTestWebAppFactory factory)
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
    public async Task GetAllProducts_ReturnsEmptyList_WhenNoProductsExist()
    {
        // Arrange
        var site = await CreateTestSite();

        // Act
        var response = await _client.GetAsync($"/api/sites/{site.Id}/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>();
        products.Should().NotBeNull();
        products.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateProduct_ReturnsCreatedProduct_WhenValidRequest()
    {
        // Arrange
        var site = await CreateTestSite();
        var command = new CreateProductCommand
        {
            SiteId = site.Id,
            ProductId = "prod-001",
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            IsPublished = true
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/sites/{site.Id}/products", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product.Should().NotBeNull();
        product!.ProductId.Should().Be("prod-001");
        product.Name.Should().Be("Test Product");
        product.Price.Should().Be(99.99m);
    }

    [Fact]
    public async Task GetProductById_ReturnsProduct_WhenProductExists()
    {
        // Arrange
        var site = await CreateTestSite();
        var createCommand = new CreateProductCommand
        {
            SiteId = site.Id,
            ProductId = "prod-002",
            Name = "Get Test Product",
            Price = 49.99m,
            IsPublished = true
        };
        var createResponse = await _client.PostAsJsonAsync($"/api/sites/{site.Id}/products", createCommand);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        // Act
        var response = await _client.GetAsync($"/api/sites/{site.Id}/products/{createdProduct!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product.Should().NotBeNull();
        product!.ProductId.Should().Be("prod-002");
    }

    [Fact]
    public async Task UpdateProduct_ReturnsUpdatedProduct_WhenValidRequest()
    {
        // Arrange
        var site = await CreateTestSite();
        var createCommand = new CreateProductCommand
        {
            SiteId = site.Id,
            ProductId = "prod-003",
            Name = "Update Test Product",
            Price = 29.99m,
            IsPublished = true
        };
        var createResponse = await _client.PostAsJsonAsync($"/api/sites/{site.Id}/products", createCommand);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        var updateCommand = new UpdateProductCommand
        {
            Id = createdProduct!.Id,
            SiteId = site.Id,
            ProductId = "prod-003-updated",
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 39.99m,
            IsPublished = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sites/{site.Id}/products/{createdProduct.Id}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product.Should().NotBeNull();
        product!.ProductId.Should().Be("prod-003-updated");
        product.Name.Should().Be("Updated Product");
        product.Price.Should().Be(39.99m);
    }

    [Fact]
    public async Task DeleteProduct_ReturnsNoContent_WhenProductExists()
    {
        // Arrange
        var site = await CreateTestSite();
        var createCommand = new CreateProductCommand
        {
            SiteId = site.Id,
            ProductId = "prod-004",
            Name = "Delete Test Product",
            Price = 19.99m,
            IsPublished = true
        };
        var createResponse = await _client.PostAsJsonAsync($"/api/sites/{site.Id}/products", createCommand);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/sites/{site.Id}/products/{createdProduct!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify soft delete
        var getResponse = await _client.GetAsync($"/api/sites/{site.Id}/products/{createdProduct.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
