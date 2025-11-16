using CMS.Domain.Entities;
using FluentAssertions;

namespace CMS.Domain.Tests.Entities;

public class TourTests
{
    [Fact]
    public void Tour_Should_Belong_To_Destination()
    {
        // Arrange
        var destinationId = Guid.NewGuid();
        var tourId = "TOUR001";
        var tourName = "City Walking Tour";
        var price = 49.99m;

        // Act
        var tour = new Tour
        {
            Id = Guid.NewGuid(),
            DestinationId = destinationId,
            TourId = tourId,
            Name = tourName,
            Price = price,
            IsPublished = true
        };

        // Assert
        tour.DestinationId.Should().Be(destinationId);
        tour.TourId.Should().Be(tourId);
        tour.Name.Should().Be(tourName);
        tour.Price.Should().Be(price);
        tour.IsPublished.Should().BeTrue();
    }

    [Fact]
    public void Destination_Should_Support_Multiple_Tours()
    {
        // Arrange
        var destination = new Destination
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            DestinationId = "paris",
            Name = "Paris",
            IsPublished = true
        };

        // Act
        destination.Tours.Add(new Tour { Id = Guid.NewGuid(), DestinationId = destination.Id, TourId = "T1", Name = "Eiffel Tower", Price = 30m });
        destination.Tours.Add(new Tour { Id = Guid.NewGuid(), DestinationId = destination.Id, TourId = "T2", Name = "Louvre Museum", Price = 25m });
        destination.Tours.Add(new Tour { Id = Guid.NewGuid(), DestinationId = destination.Id, TourId = "T3", Name = "Seine River Cruise", Price = 40m });

        // Assert
        destination.Tours.Should().HaveCount(3);
        destination.Tours.Sum(t => t.Price).Should().Be(95m);
    }
}
