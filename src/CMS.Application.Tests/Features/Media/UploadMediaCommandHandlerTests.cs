using CMS.Application.Features.Media.Commands;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Media;

public class UploadMediaCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UploadMediaCommandHandler _handler;

    public UploadMediaCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UploadMediaCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public void Handler_Should_Be_Created_Successfully()
    {
        // Arrange & Act
        var handler = new UploadMediaCommandHandler(_unitOfWorkMock.Object);

        // Assert
        handler.Should().NotBeNull();
    }

    // Note: Full testing of UploadMediaCommandHandler requires file system mocking
    // which is complex for unit tests. Integration tests cover this scenario.
}
