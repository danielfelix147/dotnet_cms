using CMS.Application.Features.Destinations.Queries;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Destinations;

public class GetDestinationByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Destination>> _destinationRepositoryMock;
    private readonly GetDestinationByIdQueryHandler _handler;

    public GetDestinationByIdQueryHandlerTests()
    {
        _destinationRepositoryMock = new Mock<IRepository<Destination>>();
        _handler = new GetDestinationByIdQueryHandler(_destinationRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Destination_When_Found()
    {
        // Arrange
        var destinationId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var destination = new Destination
        {
            Id = destinationId,
            SiteId = siteId,
            DestinationId = "paris",
            Name = "Paris",
            Description = "City of Light",
            IsDeleted = false
        };

        var query = new GetDestinationByIdQuery { Id = destinationId, SiteId = siteId };

        _destinationRepositoryMock.Setup(r => r.GetByIdAsync(destinationId)).ReturnsAsync(destination);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(destinationId);
        result.DestinationId.Should().Be("paris");
        result.Destination.Should().Be("Paris");
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Destination_Not_Found()
    {
        // Arrange
        var query = new GetDestinationByIdQuery
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid()
        };

        _destinationRepositoryMock.Setup(r => r.GetByIdAsync(query.Id)).ReturnsAsync((Destination?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_SiteId_Mismatch()
    {
        // Arrange
        var destinationId = Guid.NewGuid();
        var destination = new Destination
        {
            Id = destinationId,
            SiteId = Guid.NewGuid(),
            DestinationId = "destination",
            Name = "Destination",
            IsDeleted = false
        };

        var query = new GetDestinationByIdQuery
        {
            Id = destinationId,
            SiteId = Guid.NewGuid() // Different SiteId
        };

        _destinationRepositoryMock.Setup(r => r.GetByIdAsync(destinationId)).ReturnsAsync(destination);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Destination_Is_Deleted()
    {
        // Arrange
        var destinationId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var destination = new Destination
        {
            Id = destinationId,
            SiteId = siteId,
            DestinationId = "deleted",
            Name = "Deleted Destination",
            IsDeleted = true
        };

        var query = new GetDestinationByIdQuery { Id = destinationId, SiteId = siteId };

        _destinationRepositoryMock.Setup(r => r.GetByIdAsync(destinationId)).ReturnsAsync(destination);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
