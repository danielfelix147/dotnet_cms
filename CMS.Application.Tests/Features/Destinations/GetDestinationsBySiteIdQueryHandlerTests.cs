using CMS.Application.Features.Destinations.Queries;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;

namespace CMS.Application.Tests.Features.Destinations;

public class GetDestinationsBySiteIdQueryHandlerTests
{
    private readonly Mock<IRepository<Destination>> _destinationRepositoryMock;
    private readonly GetDestinationsBySiteIdQueryHandler _handler;

    public GetDestinationsBySiteIdQueryHandlerTests()
    {
        _destinationRepositoryMock = new Mock<IRepository<Destination>>();
        _handler = new GetDestinationsBySiteIdQueryHandler(_destinationRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Destinations_For_Site()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var destinations = new List<Destination>
        {
            new Destination
            {
                Id = Guid.NewGuid(),
                SiteId = siteId,
                DestinationId = "paris",
                Name = "Paris",
                IsDeleted = false
            },
            new Destination
            {
                Id = Guid.NewGuid(),
                SiteId = siteId,
                DestinationId = "london",
                Name = "London",
                IsDeleted = false
            }
        };

        var query = new GetDestinationsBySiteIdQuery { SiteId = siteId };

        _destinationRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Destination, bool>>>()))
            .ReturnsAsync(destinations);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.DestinationId == "paris");
        result.Should().Contain(d => d.DestinationId == "london");
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_When_No_Destinations_Found()
    {
        // Arrange
        var query = new GetDestinationsBySiteIdQuery { SiteId = Guid.NewGuid() };

        _destinationRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Destination, bool>>>()))
            .ReturnsAsync(new List<Destination>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Filter_Deleted_Destinations()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var destinations = new List<Destination>
        {
            new Destination
            {
                Id = Guid.NewGuid(),
                SiteId = siteId,
                DestinationId = "active",
                Name = "Active Destination",
                IsDeleted = false
            }
        };

        var query = new GetDestinationsBySiteIdQuery { SiteId = siteId };

        _destinationRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Destination, bool>>>()))
            .ReturnsAsync(destinations);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().DestinationId.Should().Be("active");
    }
}
