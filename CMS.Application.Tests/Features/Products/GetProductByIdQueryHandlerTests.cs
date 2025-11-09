using CMS.Application.Features.Products.Queries;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Products;

public class GetProductByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        _productRepositoryMock = new Mock<IRepository<Product>>();
        _handler = new GetProductByIdQueryHandler(_productRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Product_When_Found()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            SiteId = siteId,
            ProductId = "premium-product",
            Name = "Premium Product",
            Price = 299.99m,
            IsDeleted = false
        };

        var query = new GetProductByIdQuery { Id = productId, SiteId = siteId };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(productId);
        result.ProductId.Should().Be("premium-product");
        result.Name.Should().Be("Premium Product");
        result.Price.Should().Be(299.99m);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Product_Not_Found()
    {
        // Arrange
        var query = new GetProductByIdQuery
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid()
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(query.Id)).ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_SiteId_Mismatch()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            SiteId = Guid.NewGuid(),
            ProductId = "product",
            Name = "Product",
            Price = 50.00m,
            IsDeleted = false
        };

        var query = new GetProductByIdQuery
        {
            Id = productId,
            SiteId = Guid.NewGuid() // Different SiteId
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Product_Is_Deleted()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            SiteId = siteId,
            ProductId = "deleted",
            Name = "Deleted Product",
            Price = 100.00m,
            IsDeleted = true
        };

        var query = new GetProductByIdQuery { Id = productId, SiteId = siteId };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
