using CMS.Domain.Entities;
using CMS.Infrastructure.Plugins;
using FluentAssertions;
using System.Text.Json;
using File = CMS.Domain.Entities.File;

namespace CMS.Infrastructure.Tests.Plugins;

public class ProductManagementPluginTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public ProductManagementPluginTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Plugin_Properties_Should_Be_Correct()
    {
        // Arrange
        var context = _fixture.CreateDbContext();
        var plugin = new ProductManagementPlugin(context);

        // Assert
        plugin.SystemName.Should().Be("ProductManagement");
        plugin.DisplayName.Should().Be("Product Management");
        plugin.Description.Should().Be("Manage products with images and files");
        plugin.Version.Should().Be("1.0.0");
    }

    [Fact]
    public async Task GetContentAsync_Should_Return_Published_Products()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var plugin = new ProductManagementPlugin(context);

        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Test Site",
            Domain = $"product-plugin-{Guid.NewGuid()}.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Sites.Add(site);
        await context.SaveChangesAsync();

        var product = new Product
        {
            Id = Guid.NewGuid(),
            SiteId = site.Id,
            ProductId = "prod-001",
            Name = "Test Product",
            Price = 99.99m,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Products.Add(product);
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
    public async Task GetContentAsync_Should_Include_Product_Images_And_Files()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var plugin = new ProductManagementPlugin(context);

        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Test Site",
            Domain = $"product-media-{Guid.NewGuid()}.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Sites.Add(site);
        await context.SaveChangesAsync();

        var product = new Product
        {
            Id = Guid.NewGuid(),
            SiteId = site.Id,
            ProductId = "prod-002",
            Name = "Premium Product",
            Price = 199.99m,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var image = new Image
        {
            Id = Guid.NewGuid(),
            EntityId = product.Id,
            EntityType = "Product",
            ImageId = "product-img",
            Location = "/images/product.jpg",
            CreatedAt = DateTime.UtcNow
        };
        context.Images.Add(image);
        await context.SaveChangesAsync();

        var file = new File
        {
            Id = Guid.NewGuid(),
            EntityId = product.Id,
            EntityType = "Product",
            FileId = "product-manual",
            Location = "/files/manual.pdf",
            CreatedAt = DateTime.UtcNow
        };
        context.Files.Add(file);
        await context.SaveChangesAsync();

        // Act
        var result = await plugin.GetContentAsync(site.Id);

        // Assert
        result.Should().NotBeNull();
        var json = JsonSerializer.Serialize(result);
        json.Should().Contain("product-img");
        json.Should().Contain("product-manual");
    }

    [Fact]
    public async Task GenerateJsonAsync_Should_Return_Valid_Json()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var plugin = new ProductManagementPlugin(context);

        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Test Site",
            Domain = $"product-json-{Guid.NewGuid()}.com",
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
    public async Task GetContentAsync_Should_Not_Return_Unpublished_Products()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var plugin = new ProductManagementPlugin(context);

        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Test Site",
            Domain = $"product-unpub-{Guid.NewGuid()}.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Sites.Add(site);
        await context.SaveChangesAsync();

        var unpublishedProduct = new Product
        {
            Id = Guid.NewGuid(),
            SiteId = site.Id,
            ProductId = "unpub-prod",
            Name = "Unpublished Product",
            Price = 49.99m,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow
        };
        context.Products.Add(unpublishedProduct);
        await context.SaveChangesAsync();

        // Act
        var result = await plugin.GetContentAsync(site.Id);

        // Assert
        result.Should().NotBeNull();
        var json = JsonSerializer.Serialize(result);
        json.Should().NotContain("Unpublished Product");
    }
}
