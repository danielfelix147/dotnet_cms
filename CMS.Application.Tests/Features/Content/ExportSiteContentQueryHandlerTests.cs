using CMS.Application.Features.Content.Queries;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Content;

public class ExportSiteContentQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISiteRepository> _siteRepositoryMock;
    private readonly ExportSiteContentQueryHandler _handler;

    public ExportSiteContentQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _siteRepositoryMock = new Mock<ISiteRepository>();
        _handler = new ExportSiteContentQueryHandler(_unitOfWorkMock.Object, _siteRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Export_Site_Content()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var site = new Site { Id = siteId, Name = "Test Site", Domain = "test.com" };
        var query = new ExportSiteContentQuery { SiteId = siteId };

        _siteRepositoryMock.Setup(r => r.GetByIdAsync(siteId)).ReturnsAsync(site);
        
        var pageRepoMock = new Mock<IRepository<Page>>();
        pageRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Page, bool>>>()))
            .ReturnsAsync(new List<Page>());
        _unitOfWorkMock.Setup(u => u.Repository<Page>()).Returns(pageRepoMock.Object);

        var productRepoMock = new Mock<IRepository<Product>>();
        productRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(new List<Product>());
        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(productRepoMock.Object);

        var destRepoMock = new Mock<IRepository<Destination>>();
        destRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Destination, bool>>>()))
            .ReturnsAsync(new List<Destination>());
        _unitOfWorkMock.Setup(u => u.Repository<Destination>()).Returns(destRepoMock.Object);

        var imageRepoMock = new Mock<IRepository<Image>>();
        imageRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Image, bool>>>()))
            .ReturnsAsync(new List<Image>());
        _unitOfWorkMock.Setup(u => u.Repository<Image>()).Returns(imageRepoMock.Object);

        var fileRepoMock = new Mock<IRepository<CMS.Domain.Entities.File>>();
        fileRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CMS.Domain.Entities.File, bool>>>()))
            .ReturnsAsync(new List<CMS.Domain.Entities.File>());
        _unitOfWorkMock.Setup(u => u.Repository<CMS.Domain.Entities.File>()).Returns(fileRepoMock.Object);

        var pageContentRepoMock = new Mock<IRepository<PageContent>>();
        pageContentRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PageContent, bool>>>()))
            .ReturnsAsync(new List<PageContent>());
        _unitOfWorkMock.Setup(u => u.Repository<PageContent>()).Returns(pageContentRepoMock.Object);

        var tourRepoMock = new Mock<IRepository<Tour>>();
        tourRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tour, bool>>>()))
            .ReturnsAsync(new List<Tour>());
        _unitOfWorkMock.Setup(u => u.Repository<Tour>()).Returns(tourRepoMock.Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SiteId.Should().Be(siteId);
        result.Name.Should().Be("Test Site");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Site_Not_Found()
    {
        // Arrange
        var query = new ExportSiteContentQuery { SiteId = Guid.NewGuid() };
        _siteRepositoryMock.Setup(r => r.GetByIdAsync(query.SiteId)).ReturnsAsync((Site?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(query, CancellationToken.None));
    }
}
