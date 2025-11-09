using CMS.Application.Mappings;
using CMS.Application.DTOs;
using CMS.Application.Features.Sites.Commands;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Sites;

public class UpdateSiteCommandHandlerTests
{
    private readonly Mock<ISiteRepository> _siteRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    
    private readonly UpdateSiteCommandHandler _handler;

    public UpdateSiteCommandHandlerTests()
    {
        _siteRepositoryMock = new Mock<ISiteRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();  
        _handler = new UpdateSiteCommandHandler(_siteRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Site_Successfully()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var command = new UpdateSiteCommand
        {
            Id = siteId,
            Name = "Updated Site",
            Domain = "updated.com",
            Description = "Updated description",
            IsActive = true
        };

        var existingSite = new Site
        {
            Id = siteId,
            Name = "Old Site",
            Domain = "old.com",
            Description = "Old description",
            IsActive = false,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        var updatedSiteDto = new SiteDto
        {
            Id = siteId,
            Name = command.Name,
            Domain = command.Domain,
            Description = command.Description,
            IsActive = command.IsActive
        };

        _siteRepositoryMock.Setup(r => r.GetByIdAsync(siteId)).ReturnsAsync(existingSite);
        _siteRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Site>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(command.Name);
        result.Domain.Should().Be(command.Domain);
        result.Description.Should().Be(command.Description);
        result.IsActive.Should().Be(command.IsActive);
        
        _siteRepositoryMock.Verify(r => r.GetByIdAsync(siteId), Times.Once);
        _siteRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Site>(s => 
            s.Name == command.Name && 
            s.Domain == command.Domain && 
            s.UpdatedAt != null)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Site_Not_Found()
    {
        // Arrange
        var command = new UpdateSiteCommand
        {
            Id = Guid.NewGuid(),
            Name = "Updated Site",
            Domain = "updated.com"
        };

        _siteRepositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Site?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _siteRepositoryMock.Verify(r => r.GetByIdAsync(command.Id), Times.Once);
        _siteRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Site>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Set_UpdatedAt_Timestamp()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var command = new UpdateSiteCommand
        {
            Id = siteId,
            Name = "Updated Site",
            Domain = "updated.com",
            IsActive = true
        };

        var existingSite = new Site
        {
            Id = siteId,
            Name = "Old Site",
            Domain = "old.com",
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = null
        };

        Site? capturedSite = null;

        _siteRepositoryMock.Setup(r => r.GetByIdAsync(siteId)).ReturnsAsync(existingSite);
        _siteRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Site>()))
            .Callback<Site>(s => capturedSite = s)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedSite.Should().NotBeNull();
        capturedSite!.UpdatedAt.Should().NotBeNull();
        capturedSite.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}
