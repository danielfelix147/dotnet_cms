using CMS.Domain.Entities;
using FluentAssertions;

namespace CMS.Domain.Tests.Entities;

public class ImageTests
{
    [Fact]
    public void Image_Should_Initialize_With_Valid_Properties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var imageId = "hero-image";
        var location = "/uploads/images/hero.jpg";
        var altText = "Hero banner image";
        var title = "Homepage Hero";
        var mimeType = "image/jpeg";
        var fileSize = 512000L;

        // Act
        var image = new Image
        {
            Id = id,
            ImageId = imageId,
            Location = location,
            AltText = altText,
            Title = title,
            MimeType = mimeType,
            FileSize = fileSize,
            EntityId = entityId,
            EntityType = "Page"
        };

        // Assert
        image.Id.Should().Be(id);
        image.ImageId.Should().Be(imageId);
        image.Location.Should().Be(location);
        image.AltText.Should().Be(altText);
        image.Title.Should().Be(title);
        image.MimeType.Should().Be(mimeType);
        image.FileSize.Should().Be(fileSize);
        image.EntityId.Should().Be(entityId);
        image.EntityType.Should().Be("Page");
    }

    [Fact]
    public void Image_Should_Support_Polymorphic_Association()
    {
        // Arrange & Act
        var pageImage = new Image
        {
            Id = Guid.NewGuid(),
            ImageId = "page-img",
            Location = "/images/page.jpg",
            EntityId = Guid.NewGuid(),
            EntityType = "Page"
        };

        var destinationImage = new Image
        {
            Id = Guid.NewGuid(),
            ImageId = "destination-img",
            Location = "/images/destination.jpg",
            EntityId = Guid.NewGuid(),
            EntityType = "Destination"
        };

        var tourImage = new Image
        {
            Id = Guid.NewGuid(),
            ImageId = "tour-img",
            Location = "/images/tour.jpg",
            EntityId = Guid.NewGuid(),
            EntityType = "Tour"
        };

        // Assert
        pageImage.EntityType.Should().Be("Page");
        destinationImage.EntityType.Should().Be("Destination");
        tourImage.EntityType.Should().Be("Tour");
    }

    [Fact]
    public void Image_Should_Allow_Nullable_Properties()
    {
        // Arrange & Act
        var image = new Image
        {
            Id = Guid.NewGuid(),
            ImageId = "simple-img",
            Location = "/images/photo.png",
            EntityId = Guid.NewGuid(),
            EntityType = "Product"
        };

        // Assert
        image.AltText.Should().BeNull();
        image.Title.Should().BeNull();
        image.MimeType.Should().BeNull();
        image.FileSize.Should().Be(0);
    }
}
