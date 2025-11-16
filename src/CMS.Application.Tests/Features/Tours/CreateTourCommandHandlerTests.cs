using CMS.Application.DTOs;
using CMS.Application.Features.Tours.Commands;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Tours;

public class CreateTourCommandHandlerTests
{
    private readonly Mock<IRepository<Tour>> _tourRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateTourCommandHandler _handler;

    public CreateTourCommandHandlerTests()
    {
        _tourRepositoryMock = new Mock<IRepository<Tour>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateTourCommandHandler(_tourRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Tour_Successfully()
    {
        // Arrange
        var command = new CreateTourCommand
        {
            DestinationId = Guid.NewGuid(),
            TourId = "tour-123",
            Name = "City Walking Tour",
            Description = "Explore the city on foot",
            Price = 49.99m,
            IsPublished = true
        };

        _tourRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Tour>())).ReturnsAsync((Tour t) => t);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DestinationId.Should().Be(command.DestinationId);
        result.TourId.Should().Be(command.TourId);
        result.Name.Should().Be(command.Name);
        result.Description.Should().Be(command.Description);
        result.Price.Should().Be(command.Price);
        result.IsPublished.Should().Be(command.IsPublished);
        _tourRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Tour>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Set_CreatedAt_Timestamp()
    {
        // Arrange
        var command = new CreateTourCommand
        {
            DestinationId = Guid.NewGuid(),
            TourId = "tour-456",
            Name = "Museum Tour",
            Price = 29.99m,
            IsPublished = false
        };

        Tour capturedTour = null!;
        _tourRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Tour>()))
            .Callback<Tour>(t => capturedTour = t)
            .ReturnsAsync((Tour t) => t);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedTour.Should().NotBeNull();
        capturedTour.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_Should_Generate_New_Id()
    {
        // Arrange
        var command = new CreateTourCommand
        {
            DestinationId = Guid.NewGuid(),
            TourId = "tour-789",
            Name = "Food Tour",
            Price = 79.99m,
            IsPublished = true
        };

        _tourRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Tour>())).ReturnsAsync((Tour t) => t);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().NotBeEmpty();
    }
}
