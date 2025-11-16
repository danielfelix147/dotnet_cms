using CMS.Application.Features.Tours.Queries;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;

namespace CMS.Application.Tests.Features.Tours;

public class GetToursByDestinationIdQueryHandlerTests
{
    private readonly Mock<IRepository<Tour>> _tourRepositoryMock;
    private readonly GetToursByDestinationIdQueryHandler _handler;

    public GetToursByDestinationIdQueryHandlerTests()
    {
        _tourRepositoryMock = new Mock<IRepository<Tour>>();
        _handler = new GetToursByDestinationIdQueryHandler(_tourRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Tours_For_Destination()
    {
        // Arrange
        var destinationId = Guid.NewGuid();
        var tours = new List<Tour>
        {
            new Tour
            {
                Id = Guid.NewGuid(),
                DestinationId = destinationId,
                TourId = "tour-1",
                Name = "Tour 1",
                Price = 99.99m,
                IsDeleted = false
            },
            new Tour
            {
                Id = Guid.NewGuid(),
                DestinationId = destinationId,
                TourId = "tour-2",
                Name = "Tour 2",
                Price = 149.99m,
                IsDeleted = false
            }
        };

        var query = new GetToursByDestinationIdQuery { DestinationId = destinationId };

        _tourRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tour, bool>>>()))
            .ReturnsAsync(tours);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.TourId == "tour-1");
        result.Should().Contain(t => t.TourId == "tour-2");
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_When_No_Tours_Found()
    {
        // Arrange
        var query = new GetToursByDestinationIdQuery { DestinationId = Guid.NewGuid() };

        _tourRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tour, bool>>>()))
            .ReturnsAsync(new List<Tour>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Filter_Deleted_Tours()
    {
        // Arrange
        var destinationId = Guid.NewGuid();
        var tours = new List<Tour>
        {
            new Tour
            {
                Id = Guid.NewGuid(),
                DestinationId = destinationId,
                TourId = "active",
                Name = "Active Tour",
                Price = 50.00m,
                IsDeleted = false
            }
        };

        var query = new GetToursByDestinationIdQuery { DestinationId = destinationId };

        _tourRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tour, bool>>>()))
            .ReturnsAsync(tours);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().TourId.Should().Be("active");
    }
}
