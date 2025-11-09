using CMS.Application.Mappings;
using CMS.Application.DTOs;
using CMS.Application.Features.Sites.Queries;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Sites;

public class GetAllSitesQueryHandlerTests
{
    private readonly Mock<ISiteRepository> _siteRepositoryMock;
    
    private readonly GetAllSitesQueryHandler _handler;

    public GetAllSitesQueryHandlerTests()
    {
        _siteRepositoryMock = new Mock<ISiteRepository>();  
        _handler = new GetAllSitesQueryHandler(_siteRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Sites()
    {
        // Arrange
        var sites = new List<Site>
        {
            new() { Id = Guid.NewGuid(), Name = "Site 1", Domain = "site1.com", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Site 2", Domain = "site2.com", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Site 3", Domain = "site3.com", IsActive = false }
        };

        var siteDtos = sites.Select(s => new SiteDto
        {
            Id = s.Id,
            Name = s.Name,
            Domain = s.Domain,
            Description = s.Description,
            IsActive = s.IsActive
        }).ToList();

        _siteRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(sites);

        var query = new GetAllSitesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(siteDtos);
        _siteRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Sites_Exist()
    {
        // Arrange
        var emptySites = new List<Site>();
        var emptySiteDtos = new List<SiteDto>();

        _siteRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(emptySites);

        var query = new GetAllSitesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _siteRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }
}
