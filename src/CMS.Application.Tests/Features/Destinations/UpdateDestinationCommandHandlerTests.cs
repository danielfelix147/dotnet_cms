using CMS.Application.Features.Destinations.Commands;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Destinations;

public class UpdateDestinationCommandHandlerTests
{
    private readonly Mock<IRepository<Destination>> _destinationRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateDestinationCommandHandler _handler;

    public UpdateDestinationCommandHandlerTests()
    {
        _destinationRepositoryMock = new Mock<IRepository<Destination>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateDestinationCommandHandler(_destinationRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Destination_Successfully()
    {
        // Arrange
        var destinationId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var existingDestination = new Destination
        {
            Id = destinationId,
            SiteId = siteId,
            DestinationId = "old-dest",
            Name = "Old Name",
            Description = "Old Description",
            IsPublished = false
        };

        var command = new UpdateDestinationCommand
        {
            Id = destinationId,
            SiteId = siteId,
            DestinationId = "new-dest",
            Name = "New Name",
            Description = "New Description",
            IsPublished = true
        };

        _destinationRepositoryMock.Setup(r => r.GetByIdAsync(destinationId)).ReturnsAsync(existingDestination);
        _destinationRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Destination>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.DestinationId.Should().Be(command.DestinationId);
        result.Destination.Should().Be(command.Name);
        result.Description.Should().Be(command.Description);
        result.IsPublished.Should().Be(command.IsPublished);
        _destinationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Destination>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Destination_Not_Found()
    {
        // Arrange
        var command = new UpdateDestinationCommand
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            DestinationId = "test",
            Name = "Test"
        };

        _destinationRepositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Destination?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _destinationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Destination>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_SiteId_Mismatch()
    {
        // Arrange
        var destinationId = Guid.NewGuid();
        var existingDestination = new Destination
        {
            Id = destinationId,
            SiteId = Guid.NewGuid(),
            DestinationId = "destination",
            Name = "Destination"
        };

        var command = new UpdateDestinationCommand
        {
            Id = destinationId,
            SiteId = Guid.NewGuid(), // Different SiteId
            DestinationId = "destination",
            Name = "Updated Destination"
        };

        _destinationRepositoryMock.Setup(r => r.GetByIdAsync(destinationId)).ReturnsAsync(existingDestination);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _destinationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Destination>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Update_Timestamp()
    {
        // Arrange
        var destinationId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var existingDestination = new Destination
        {
            Id = destinationId,
            SiteId = siteId,
            DestinationId = "destination",
            Name = "Destination",
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var command = new UpdateDestinationCommand
        {
            Id = destinationId,
            SiteId = siteId,
            DestinationId = "destination",
            Name = "Updated Destination"
        };

        Destination updatedDestination = null!;
        _destinationRepositoryMock.Setup(r => r.GetByIdAsync(destinationId)).ReturnsAsync(existingDestination);
        _destinationRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Destination>()))
            .Callback<Destination>(d => updatedDestination = d)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        updatedDestination.Should().NotBeNull();
        updatedDestination.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
