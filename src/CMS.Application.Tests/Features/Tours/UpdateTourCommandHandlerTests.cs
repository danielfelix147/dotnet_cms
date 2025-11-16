using CMS.Application.Features.Tours.Commands;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Tours;

public class UpdateTourCommandHandlerTests
{
    private readonly Mock<IRepository<Tour>> _tourRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateTourCommandHandler _handler;

    public UpdateTourCommandHandlerTests()
    {
        _tourRepositoryMock = new Mock<IRepository<Tour>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateTourCommandHandler(_tourRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Tour_Successfully()
    {
        // Arrange
        var tourId = Guid.NewGuid();
        var destinationId = Guid.NewGuid();
        var existingTour = new Tour
        {
            Id = tourId,
            DestinationId = destinationId,
            TourId = "old-tour",
            Name = "Old Name",
            Description = "Old Description",
            Price = 50.00m,
            IsPublished = false
        };

        var command = new UpdateTourCommand
        {
            Id = tourId,
            DestinationId = destinationId,
            TourId = "new-tour",
            Name = "New Name",
            Description = "New Description",
            Price = 99.99m,
            IsPublished = true
        };

        _tourRepositoryMock.Setup(r => r.GetByIdAsync(tourId)).ReturnsAsync(existingTour);
        _tourRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Tour>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.TourId.Should().Be(command.TourId);
        result.Name.Should().Be(command.Name);
        result.Description.Should().Be(command.Description);
        result.Price.Should().Be(command.Price);
        result.IsPublished.Should().Be(command.IsPublished);
        _tourRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Tour>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Tour_Not_Found()
    {
        // Arrange
        var command = new UpdateTourCommand
        {
            Id = Guid.NewGuid(),
            DestinationId = Guid.NewGuid(),
            TourId = "test",
            Name = "Test",
            Price = 10.00m
        };

        _tourRepositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Tour?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _tourRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Tour>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_DestinationId_Mismatch()
    {
        // Arrange
        var tourId = Guid.NewGuid();
        var existingTour = new Tour
        {
            Id = tourId,
            DestinationId = Guid.NewGuid(),
            TourId = "tour",
            Name = "Tour",
            Price = 25.00m
        };

        var command = new UpdateTourCommand
        {
            Id = tourId,
            DestinationId = Guid.NewGuid(), // Different DestinationId
            TourId = "tour",
            Name = "Updated Tour",
            Price = 30.00m
        };

        _tourRepositoryMock.Setup(r => r.GetByIdAsync(tourId)).ReturnsAsync(existingTour);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _tourRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Tour>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Update_Timestamp()
    {
        // Arrange
        var tourId = Guid.NewGuid();
        var destinationId = Guid.NewGuid();
        var existingTour = new Tour
        {
            Id = tourId,
            DestinationId = destinationId,
            TourId = "tour",
            Name = "Tour",
            Price = 15.00m,
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var command = new UpdateTourCommand
        {
            Id = tourId,
            DestinationId = destinationId,
            TourId = "tour",
            Name = "Updated Tour",
            Price = 20.00m
        };

        Tour updatedTour = null!;
        _tourRepositoryMock.Setup(r => r.GetByIdAsync(tourId)).ReturnsAsync(existingTour);
        _tourRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Tour>()))
            .Callback<Tour>(t => updatedTour = t)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        updatedTour.Should().NotBeNull();
        updatedTour.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
