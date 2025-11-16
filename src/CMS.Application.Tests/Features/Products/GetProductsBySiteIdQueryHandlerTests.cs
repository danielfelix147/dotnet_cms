using CMS.Application.Features.Products.Queries;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;

namespace CMS.Application.Tests.Features.Products;

public class GetProductsBySiteIdQueryHandlerTests
{
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly GetProductsBySiteIdQueryHandler _handler;

    public GetProductsBySiteIdQueryHandlerTests()
    {
        _productRepositoryMock = new Mock<IRepository<Product>>();
        _handler = new GetProductsBySiteIdQueryHandler(_productRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Products_For_Site()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var products = new List<Product>
        {
            new Product
            {
                Id = Guid.NewGuid(),
                SiteId = siteId,
                ProductId = "product-1",
                Name = "Product 1",
                Price = 99.99m,
                IsDeleted = false
            },
            new Product
            {
                Id = Guid.NewGuid(),
                SiteId = siteId,
                ProductId = "product-2",
                Name = "Product 2",
                Price = 149.99m,
                IsDeleted = false
            }
        };

        var query = new GetProductsBySiteIdQuery { SiteId = siteId };

        _productRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync(products);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.ProductId == "product-1");
        result.Should().Contain(p => p.ProductId == "product-2");
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_When_No_Products_Found()
    {
        // Arrange
        var query = new GetProductsBySiteIdQuery { SiteId = Guid.NewGuid() };

        _productRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync(new List<Product>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Filter_Deleted_Products()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var products = new List<Product>
        {
            new Product
            {
                Id = Guid.NewGuid(),
                SiteId = siteId,
                ProductId = "active",
                Name = "Active Product",
                Price = 50.00m,
                IsDeleted = false
            }
        };

        var query = new GetProductsBySiteIdQuery { SiteId = siteId };

        _productRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync(products);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().ProductId.Should().Be("active");
    }
}
