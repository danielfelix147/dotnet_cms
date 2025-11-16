using CMS.Application.Mappings;
using CMS.Application.DTOs;
using CMS.Application.Features.Sites.Queries;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Sites;

public class GetSiteByIdQueryHandlerTests
{
    private readonly Mock<ISiteRepository> _siteRepositoryMock;
    
    private readonly GetSiteByIdQueryHandler _handler;

    public GetSiteByIdQueryHandlerTests()
    {
        _siteRepositoryMock = new Mock<ISiteRepository>();  
        _handler = new GetSiteByIdQueryHandler(_siteRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Site_When_Found()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var site = new Site
        {
            Id = siteId,
            Name = "Test Site",
            Domain = "testsite.com",
            Description = "A test site",
            IsActive = true
        };

        var siteDto = new SiteDto
        {
            Id = site.Id,
            Name = site.Name,
            Domain = site.Domain,
            Description = site.Description,
            IsActive = site.IsActive
        };

        _siteRepositoryMock.Setup(r => r.GetByIdAsync(siteId)).ReturnsAsync(site);

        var query = new GetSiteByIdQuery { SiteId = siteId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(siteId);
        result.Name.Should().Be("Test Site");
        result.Domain.Should().Be("testsite.com");
        _siteRepositoryMock.Verify(r => r.GetByIdAsync(siteId), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Site_Not_Found()
    {
        // Arrange
        var siteId = Guid.NewGuid();

        _siteRepositoryMock.Setup(r => r.GetByIdAsync(siteId)).ReturnsAsync((Site?)null);

        var query = new GetSiteByIdQuery { SiteId = siteId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _siteRepositoryMock.Verify(r => r.GetByIdAsync(siteId), Times.Once);
    }
}
