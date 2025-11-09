using CMS.Application.Features.Products.Commands;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Products;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _productRepositoryMock = new Mock<IRepository<Product>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateProductCommandHandler(_productRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Product_Successfully()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var existingProduct = new Product
        {
            Id = productId,
            SiteId = siteId,
            ProductId = "old-product",
            Name = "Old Name",
            Description = "Old Description",
            Price = 50.00m,
            IsPublished = false
        };

        var command = new UpdateProductCommand
        {
            Id = productId,
            SiteId = siteId,
            ProductId = "new-product",
            Name = "New Name",
            Description = "New Description",
            Price = 99.99m,
            IsPublished = true
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(existingProduct);
        _productRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.ProductId.Should().Be(command.ProductId);
        result.Name.Should().Be(command.Name);
        result.Description.Should().Be(command.Description);
        result.Price.Should().Be(command.Price);
        result.IsPublished.Should().Be(command.IsPublished);
        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Product_Not_Found()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            ProductId = "test",
            Name = "Test",
            Price = 10.00m
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_SiteId_Mismatch()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingProduct = new Product
        {
            Id = productId,
            SiteId = Guid.NewGuid(),
            ProductId = "product",
            Name = "Product",
            Price = 25.00m
        };

        var command = new UpdateProductCommand
        {
            Id = productId,
            SiteId = Guid.NewGuid(), // Different SiteId
            ProductId = "product",
            Name = "Updated Product",
            Price = 30.00m
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Update_Timestamp()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var existingProduct = new Product
        {
            Id = productId,
            SiteId = siteId,
            ProductId = "product",
            Name = "Product",
            Price = 15.00m,
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var command = new UpdateProductCommand
        {
            Id = productId,
            SiteId = siteId,
            ProductId = "product",
            Name = "Updated Product",
            Price = 20.00m
        };

        Product updatedProduct = null!;
        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(existingProduct);
        _productRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>()))
            .Callback<Product>(p => updatedProduct = p)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        updatedProduct.Should().NotBeNull();
        updatedProduct.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
