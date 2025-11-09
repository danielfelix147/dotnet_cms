using CMS.Domain.Entities;
using FluentAssertions;

namespace CMS.Domain.Tests.Entities;

public class ProductTests
{
    [Fact]
    public void Product_Should_Initialize_With_Valid_Price()
    {
        // Arrange
        var price = 99.99m;

        // Act
        var product = new Product
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            ProductId = "PROD001",
            Name = "Test Product",
            Price = price,
            IsPublished = true
        };

        // Assert
        product.Price.Should().Be(price);
        product.Name.Should().Be("Test Product");
        product.ProductId.Should().Be("PROD001");
    }

    [Fact]
    public void Product_Price_Should_Support_Decimal_Precision()
    {
        // Arrange & Act
        var product1 = new Product { Id = Guid.NewGuid(), SiteId = Guid.NewGuid(), ProductId = "P1", Name = "Product 1", Price = 10.50m };
        var product2 = new Product { Id = Guid.NewGuid(), SiteId = Guid.NewGuid(), ProductId = "P2", Name = "Product 2", Price = 999.99m };
        var product3 = new Product { Id = Guid.NewGuid(), SiteId = Guid.NewGuid(), ProductId = "P3", Name = "Product 3", Price = 0.01m };

        // Assert
        product1.Price.Should().Be(10.50m);
        product2.Price.Should().Be(999.99m);
        product3.Price.Should().Be(0.01m);
    }
}
