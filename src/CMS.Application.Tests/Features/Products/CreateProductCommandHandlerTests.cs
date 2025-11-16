using CMS.Application.DTOs;
using CMS.Application.Features.Products.Commands;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Products;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _productRepositoryMock = new Mock<IRepository<Product>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateProductCommandHandler(_productRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Product_Successfully()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            SiteId = Guid.NewGuid(),
            ProductId = "product-123",
            Name = "Test Product",
            Description = "A great product",
            Price = 99.99m,
            IsPublished = true
        };

        _productRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>())).ReturnsAsync((Product p) => p);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ProductId.Should().Be(command.ProductId);
        result.Name.Should().Be(command.Name);
        result.Description.Should().Be(command.Description);
        result.Price.Should().Be(command.Price);
        result.IsPublished.Should().Be(command.IsPublished);
        _productRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Set_CreatedAt_Timestamp()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            SiteId = Guid.NewGuid(),
            ProductId = "product-456",
            Name = "Another Product",
            Price = 49.99m,
            IsPublished = false
        };

        Product capturedProduct = null!;
        _productRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(p => capturedProduct = p)
            .ReturnsAsync((Product p) => p);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedProduct.Should().NotBeNull();
        capturedProduct.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_Should_Generate_New_Id()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            SiteId = Guid.NewGuid(),
            ProductId = "product-789",
            Name = "Premium Product",
            Price = 199.99m,
            IsPublished = true
        };

        _productRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>())).ReturnsAsync((Product p) => p);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().NotBeEmpty();
    }
}
