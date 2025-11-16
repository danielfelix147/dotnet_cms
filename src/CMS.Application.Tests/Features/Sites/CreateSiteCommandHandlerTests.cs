using CMS.Application.Mappings;
using CMS.Application.DTOs;
using CMS.Application.Features.Sites.Commands;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Sites;

public class CreateSiteCommandHandlerTests
{
    private readonly Mock<ISiteRepository> _siteRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    
    private readonly CreateSiteCommandHandler _handler;

    public CreateSiteCommandHandlerTests()
    {
        _siteRepositoryMock = new Mock<ISiteRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();  
        _handler = new CreateSiteCommandHandler(_siteRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Site_Successfully()
    {
        // Arrange
        var command = new CreateSiteCommand
        {
            Name = "Test Site",
            Domain = "testsite.com",
            Description = "A test site"
        };

        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Domain = command.Domain,
            Description = command.Description,
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
        _siteRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Site>())).ReturnsAsync(site);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be(command.Name);
        result.Domain.Should().Be(command.Domain);
        result.IsActive.Should().BeTrue();
        _siteRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Site>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Map_Command_Properties_To_Site()
    {
        // Arrange
        var command = new CreateSiteCommand
        {
            Name = "My Website",
            Domain = "mywebsite.com",
            Description = "My personal website"
        };

        var siteDto = new SiteDto
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Domain = command.Domain,
            Description = command.Description,
            IsActive = true
        };
        _siteRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Site>())).ReturnsAsync(new Site());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(command.Name);
        result.Domain.Should().Be(command.Domain);
        result.Description.Should().Be(command.Description);
    }
}
