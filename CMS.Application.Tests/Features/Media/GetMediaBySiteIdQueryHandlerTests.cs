using CMS.Application.Features.Media.Queries;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;

namespace CMS.Application.Tests.Features.Media;

public class GetMediaBySiteIdQueryHandlerTests
{
    private readonly Mock<IRepository<Image>> _imageRepositoryMock;
    private readonly Mock<IRepository<CMS.Domain.Entities.File>> _fileRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly GetMediaBySiteIdQueryHandler _handler;

    public GetMediaBySiteIdQueryHandlerTests()
    {
        _imageRepositoryMock = new Mock<IRepository<Image>>();
        _fileRepositoryMock = new Mock<IRepository<CMS.Domain.Entities.File>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Repository<Image>()).Returns(_imageRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<CMS.Domain.Entities.File>()).Returns(_fileRepositoryMock.Object);
        _handler = new GetMediaBySiteIdQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Media_For_Site()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var images = new List<Image>
        {
            new Image 
            { 
                Id = Guid.NewGuid(), 
                EntityId = siteId, 
                EntityType = "Site", 
                Location = "/uploads/img1.jpg",
                ImageId = "img1",
                Title = "Image 1",
                MimeType = "image/jpeg",
                FileSize = 1000,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "test"
            },
            new Image 
            { 
                Id = Guid.NewGuid(), 
                EntityId = siteId, 
                EntityType = "Site", 
                Location = "/uploads/img2.jpg",
                ImageId = "img2",
                Title = "Image 2",
                MimeType = "image/png",
                FileSize = 2000,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "test"
            }
        };

        var query = new GetMediaBySiteIdQuery { SiteId = siteId };

        _imageRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(images);
        _fileRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<CMS.Domain.Entities.File>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }
}
