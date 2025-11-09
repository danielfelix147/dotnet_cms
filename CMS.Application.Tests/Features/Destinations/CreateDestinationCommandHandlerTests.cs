using CMS.Application.DTOs;
using CMS.Application.Features.Destinations.Commands;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Destinations;

public class CreateDestinationCommandHandlerTests
{
    private readonly Mock<IRepository<Destination>> _destinationRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateDestinationCommandHandler _handler;

    public CreateDestinationCommandHandlerTests()
    {
        _destinationRepositoryMock = new Mock<IRepository<Destination>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateDestinationCommandHandler(_destinationRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Destination_Successfully()
    {
        // Arrange
        var command = new CreateDestinationCommand
        {
            SiteId = Guid.NewGuid(),
            DestinationId = "paris",
            Name = "Paris",
            Description = "The City of Light",
            IsPublished = true
        };

        _destinationRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Destination>())).ReturnsAsync((Destination d) => d);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SiteId.Should().Be(command.SiteId);
        result.DestinationId.Should().Be(command.DestinationId);
        result.Destination.Should().Be(command.Name);
        result.Description.Should().Be(command.Description);
        result.IsPublished.Should().Be(command.IsPublished);
        _destinationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Destination>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Set_CreatedAt_Timestamp()
    {
        // Arrange
        var command = new CreateDestinationCommand
        {
            SiteId = Guid.NewGuid(),
            DestinationId = "tokyo",
            Name = "Tokyo",
            IsPublished = false
        };

        Destination capturedDestination = null!;
        _destinationRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Destination>()))
            .Callback<Destination>(d => capturedDestination = d)
            .ReturnsAsync((Destination d) => d);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedDestination.Should().NotBeNull();
        capturedDestination.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_Should_Generate_New_Id()
    {
        // Arrange
        var command = new CreateDestinationCommand
        {
            SiteId = Guid.NewGuid(),
            DestinationId = "london",
            Name = "London",
            IsPublished = true
        };

        _destinationRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Destination>())).ReturnsAsync((Destination d) => d);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().NotBeEmpty();
    }
}
