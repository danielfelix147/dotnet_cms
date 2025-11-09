using CMS.Domain.Entities;
using FluentAssertions;

namespace CMS.Domain.Tests.Entities;

public class DestinationTests
{
    [Fact]
    public void Destination_Should_Initialize_With_Valid_Properties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var destinationId = "paris";
        var name = "Paris, France";
        var description = "The City of Light";

        // Act
        var destination = new Destination
        {
            Id = id,
            SiteId = siteId,
            DestinationId = destinationId,
            Name = name,
            Description = description,
            IsPublished = true
        };

        // Assert
        destination.Id.Should().Be(id);
        destination.SiteId.Should().Be(siteId);
        destination.DestinationId.Should().Be(destinationId);
        destination.Name.Should().Be(name);
        destination.Description.Should().Be(description);
        destination.IsPublished.Should().BeTrue();
        destination.Tours.Should().NotBeNull();
    }

    [Fact]
    public void Destination_Should_Allow_Adding_Tours()
    {
        // Arrange
        var destination = new Destination
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            DestinationId = "london",
            Name = "London, UK"
        };
        var tour = new Tour
        {
            Id = Guid.NewGuid(),
            DestinationId = destination.Id,
            TourId = "london-highlights",
            Name = "London Highlights"
        };

        // Act
        destination.Tours.Add(tour);

        // Assert
        destination.Tours.Should().HaveCount(1);
        destination.Tours.First().TourId.Should().Be("london-highlights");
        destination.Tours.First().Name.Should().Be("London Highlights");
    }

    [Fact]
    public void Destination_Should_Support_Multiple_Tours()
    {
        // Arrange
        var destination = new Destination
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            DestinationId = "rome",
            Name = "Rome, Italy"
        };

        // Act
        destination.Tours.Add(new Tour { Id = Guid.NewGuid(), DestinationId = destination.Id, TourId = "colosseum", Name = "Colosseum Tour" });
        destination.Tours.Add(new Tour { Id = Guid.NewGuid(), DestinationId = destination.Id, TourId = "vatican", Name = "Vatican Tour" });
        destination.Tours.Add(new Tour { Id = Guid.NewGuid(), DestinationId = destination.Id, TourId = "trevi", Name = "Trevi Fountain Walk" });

        // Assert
        destination.Tours.Should().HaveCount(3);
        destination.Tours.Select(t => t.TourId).Should().Contain(new[] { "colosseum", "vatican", "trevi" });
    }
}
