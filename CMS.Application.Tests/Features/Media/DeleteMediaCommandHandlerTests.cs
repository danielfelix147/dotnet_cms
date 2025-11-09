using CMS.Application.Features.Media.Commands;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Media;

public class DeleteMediaCommandHandlerTests
{
    private readonly Mock<IRepository<Image>> _imageRepositoryMock;
    private readonly Mock<IRepository<CMS.Domain.Entities.File>> _fileRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteMediaCommandHandler _handler;

    public DeleteMediaCommandHandlerTests()
    {
        _imageRepositoryMock = new Mock<IRepository<Image>>();
        _fileRepositoryMock = new Mock<IRepository<CMS.Domain.Entities.File>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Repository<Image>()).Returns(_imageRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<CMS.Domain.Entities.File>()).Returns(_fileRepositoryMock.Object);
        _handler = new DeleteMediaCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Delete_Image_Successfully()
    {
        // Arrange
        var mediaId = Guid.NewGuid();
        var image = new Image { Id = mediaId, Location = "/uploads/test.jpg" };
        var command = new DeleteMediaCommand { Id = mediaId };

        _imageRepositoryMock.Setup(r => r.GetByIdAsync(mediaId)).ReturnsAsync(image);
        _imageRepositoryMock.Setup(r => r.DeleteAsync(image)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _imageRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Image>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_False_When_Media_Not_Found()
    {
        // Arrange
        var command = new DeleteMediaCommand { Id = Guid.NewGuid() };
        _imageRepositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Image?)null);
        _fileRepositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((CMS.Domain.Entities.File?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}
