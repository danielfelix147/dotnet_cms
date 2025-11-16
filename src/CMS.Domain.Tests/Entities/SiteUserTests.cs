using CMS.Domain.Entities;
using FluentAssertions;

namespace CMS.Domain.Tests.Entities;

public class SiteUserTests
{
    [Fact]
    public void SiteUser_Should_Initialize_With_Valid_Properties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var role = "SiteAdmin";

        // Act
        var siteUser = new SiteUser
        {
            Id = id,
            SiteId = siteId,
            UserId = userId,
            Role = role
        };

        // Assert
        siteUser.Id.Should().Be(id);
        siteUser.SiteId.Should().Be(siteId);
        siteUser.UserId.Should().Be(userId);
        siteUser.Role.Should().Be(role);
    }

    [Fact]
    public void SiteUser_Should_Support_Different_Roles()
    {
        // Arrange
        var siteId = Guid.NewGuid();

        // Act
        var admin = new SiteUser
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            UserId = Guid.NewGuid().ToString(),
            Role = "SiteAdmin"
        };

        var editor = new SiteUser
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            UserId = Guid.NewGuid().ToString(),
            Role = "SiteEditor"
        };

        var viewer = new SiteUser
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            UserId = Guid.NewGuid().ToString(),
            Role = "SiteViewer"
        };

        // Assert
        admin.Role.Should().Be("SiteAdmin");
        editor.Role.Should().Be("SiteEditor");
        viewer.Role.Should().Be("SiteViewer");
    }

    [Fact]
    public void SiteUser_Should_Use_String_UserId_For_IdentityUser()
    {
        // Arrange
        var identityUserId = "550e8400-e29b-41d4-a716-446655440000"; // Example IdentityUser.Id

        // Act
        var siteUser = new SiteUser
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            UserId = identityUserId,
            Role = "SiteEditor"
        };

        // Assert
        siteUser.UserId.Should().Be(identityUserId);
        siteUser.UserId.Should().BeOfType<string>();
    }
}
