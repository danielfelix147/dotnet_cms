using CMS.Application.Features.Destinations.Commands;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Destinations;

public class DeleteDestinationCommandHandlerTests
{
    private readonly Mock<IRepository<Destination>> _destinationRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteDestinationCommandHandler _handler;

    public DeleteDestinationCommandHandlerTests()
    {
        _destinationRepositoryMock = new Mock<IRepository<Destination>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new DeleteDestinationCommandHandler(_destinationRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Delete_Destination_Successfully()
    {
        // Arrange
        var destinationId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var existingDestination = new Destination
        {
            Id = destinationId,
            SiteId = siteId,
            DestinationId = "test-destination",
            Name = "Test Destination",
            IsDeleted = false
        };

        var command = new DeleteDestinationCommand { Id = destinationId, SiteId = siteId };

        Destination deletedDestination = null!;
        _destinationRepositoryMock.Setup(r => r.GetByIdAsync(destinationId)).ReturnsAsync(existingDestination);
        _destinationRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Destination>()))
            .Callback<Destination>(d => deletedDestination = d)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        deletedDestination.Should().NotBeNull();
        deletedDestination.IsDeleted.Should().BeTrue();
        deletedDestination.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        _destinationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Destination>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_False_When_Destination_Not_Found()
    {
        // Arrange
        var command = new DeleteDestinationCommand
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid()
        };

        _destinationRepositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Destination?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _destinationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Destination>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_False_When_SiteId_Mismatch()
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

        var command = new DeleteDestinationCommand
        {
            Id = destinationId,
            SiteId = Guid.NewGuid() // Different SiteId
        };

        _destinationRepositoryMock.Setup(r => r.GetByIdAsync(destinationId)).ReturnsAsync(existingDestination);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _destinationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Destination>()), Times.Never);
    }
}
