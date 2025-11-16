using CMS.Application.Features.Tours.Commands;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Tours;

public class DeleteTourCommandHandlerTests
{
    private readonly Mock<IRepository<Tour>> _tourRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteTourCommandHandler _handler;

    public DeleteTourCommandHandlerTests()
    {
        _tourRepositoryMock = new Mock<IRepository<Tour>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new DeleteTourCommandHandler(_tourRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Delete_Tour_Successfully()
    {
        // Arrange
        var tourId = Guid.NewGuid();
        var destinationId = Guid.NewGuid();
        var existingTour = new Tour
        {
            Id = tourId,
            DestinationId = destinationId,
            TourId = "test-tour",
            Name = "Test Tour",
            Price = 50.00m,
            IsDeleted = false
        };

        var command = new DeleteTourCommand { Id = tourId, DestinationId = destinationId };

        Tour deletedTour = null!;
        _tourRepositoryMock.Setup(r => r.GetByIdAsync(tourId)).ReturnsAsync(existingTour);
        _tourRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Tour>()))
            .Callback<Tour>(t => deletedTour = t)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        deletedTour.Should().NotBeNull();
        deletedTour.IsDeleted.Should().BeTrue();
        deletedTour.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        _tourRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Tour>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_False_When_Tour_Not_Found()
    {
        // Arrange
        var command = new DeleteTourCommand
        {
            Id = Guid.NewGuid(),
            DestinationId = Guid.NewGuid()
        };

        _tourRepositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Tour?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _tourRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Tour>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_False_When_DestinationId_Mismatch()
    {
        // Arrange
        var tourId = Guid.NewGuid();
        var existingTour = new Tour
        {
            Id = tourId,
            DestinationId = Guid.NewGuid(),
            TourId = "tour",
            Name = "Tour",
            Price = 30.00m
        };

        var command = new DeleteTourCommand
        {
            Id = tourId,
            DestinationId = Guid.NewGuid() // Different DestinationId
        };

        _tourRepositoryMock.Setup(r => r.GetByIdAsync(tourId)).ReturnsAsync(existingTour);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _tourRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Tour>()), Times.Never);
    }
}
