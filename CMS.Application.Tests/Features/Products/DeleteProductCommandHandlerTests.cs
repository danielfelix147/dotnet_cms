using CMS.Application.Features.Products.Commands;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Products;

public class DeleteProductCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _productRepositoryMock = new Mock<IRepository<Product>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new DeleteProductCommandHandler(_productRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Delete_Product_Successfully()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var existingProduct = new Product
        {
            Id = productId,
            SiteId = siteId,
            ProductId = "test-product",
            Name = "Test Product",
            Price = 50.00m,
            IsDeleted = false
        };

        var command = new DeleteProductCommand { Id = productId, SiteId = siteId };

        Product deletedProduct = null!;
        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(existingProduct);
        _productRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>()))
            .Callback<Product>(p => deletedProduct = p)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        deletedProduct.Should().NotBeNull();
        deletedProduct.IsDeleted.Should().BeTrue();
        deletedProduct.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_False_When_Product_Not_Found()
    {
        // Arrange
        var command = new DeleteProductCommand
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid()
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_False_When_SiteId_Mismatch()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingProduct = new Product
        {
            Id = productId,
            SiteId = Guid.NewGuid(),
            ProductId = "product",
            Name = "Product",
            Price = 30.00m
        };

        var command = new DeleteProductCommand
        {
            Id = productId,
            SiteId = Guid.NewGuid() // Different SiteId
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }
}
