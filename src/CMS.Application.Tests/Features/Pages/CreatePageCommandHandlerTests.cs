using CMS.Application.DTOs;
using CMS.Application.Features.Pages.Commands;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Pages;

public class CreatePageCommandHandlerTests
{
    private readonly Mock<IRepository<Page>> _pageRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreatePageCommandHandler _handler;

    public CreatePageCommandHandlerTests()
    {
        _pageRepositoryMock = new Mock<IRepository<Page>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreatePageCommandHandler(_pageRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Page_Successfully()
    {
        // Arrange
        var command = new CreatePageCommand
        {
            SiteId = Guid.NewGuid(),
            PageId = "home",
            Title = "Home Page",
            Description = "Welcome to our home page",
            IsPublished = true
        };

        _pageRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Page>())).ReturnsAsync((Page p) => p);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SiteId.Should().Be(command.SiteId);
        result.PageId.Should().Be(command.PageId);
        result.Title.Should().Be(command.Title);
        result.Description.Should().Be(command.Description);
        result.IsPublished.Should().Be(command.IsPublished);
        _pageRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Page>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Set_CreatedAt_Timestamp()
    {
        // Arrange
        var command = new CreatePageCommand
        {
            SiteId = Guid.NewGuid(),
            PageId = "about",
            Title = "About Us",
            IsPublished = false
        };

        Page capturedPage = null!;
        _pageRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Page>()))
            .Callback<Page>(p => capturedPage = p)
            .ReturnsAsync((Page p) => p);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedPage.Should().NotBeNull();
        capturedPage.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_Should_Generate_New_Id()
    {
        // Arrange
        var command = new CreatePageCommand
        {
            SiteId = Guid.NewGuid(),
            PageId = "contact",
            Title = "Contact Us",
            IsPublished = true
        };

        _pageRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Page>())).ReturnsAsync((Page p) => p);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().NotBeEmpty();
    }
}
