using FluentAssertions;
using File = CMS.Domain.Entities.File;

namespace CMS.Domain.Tests.Entities;

public class FileTests
{
    [Fact]
    public void File_Should_Initialize_With_Valid_Properties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var fileId = "document1";
        var location = "/uploads/files/document.pdf";
        var title = "User Manual";
        var mimeType = "application/pdf";
        var fileSize = 1024000L;

        // Act
        var file = new File
        {
            Id = id,
            FileId = fileId,
            Location = location,
            Title = title,
            MimeType = mimeType,
            FileSize = fileSize,
            EntityId = entityId,
            EntityType = "Product"
        };

        // Assert
        file.Id.Should().Be(id);
        file.FileId.Should().Be(fileId);
        file.Location.Should().Be(location);
        file.Title.Should().Be(title);
        file.MimeType.Should().Be(mimeType);
        file.FileSize.Should().Be(fileSize);
        file.EntityId.Should().Be(entityId);
        file.EntityType.Should().Be("Product");
    }

    [Fact]
    public void File_Should_Support_Polymorphic_Association()
    {
        // Arrange & Act
        var pageFile = new File
        {
            Id = Guid.NewGuid(),
            FileId = "page-doc",
            Location = "/files/page.pdf",
            EntityId = Guid.NewGuid(),
            EntityType = "Page"
        };

        var productFile = new File
        {
            Id = Guid.NewGuid(),
            FileId = "product-manual",
            Location = "/files/manual.pdf",
            EntityId = Guid.NewGuid(),
            EntityType = "Product"
        };

        // Assert
        pageFile.EntityType.Should().Be("Page");
        productFile.EntityType.Should().Be("Product");
    }

    [Fact]
    public void File_Should_Allow_Nullable_Properties()
    {
        // Arrange & Act
        var file = new File
        {
            Id = Guid.NewGuid(),
            FileId = "simple-file",
            Location = "/files/data.bin",
            EntityId = Guid.NewGuid(),
            EntityType = "Tour"
        };

        // Assert
        file.Title.Should().BeNull();
        file.MimeType.Should().BeNull();
        file.FileSize.Should().Be(0);
    }
}
