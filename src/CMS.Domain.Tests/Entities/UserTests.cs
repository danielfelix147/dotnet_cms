using CMS.Domain.Entities;
using FluentAssertions;

namespace CMS.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void User_Should_Initialize_With_Valid_Properties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var email = "john.doe@example.com";
        var firstName = "John";
        var lastName = "Doe";
        var role = "Admin";
        var isActive = true;

        // Act
        var user = new User
        {
            Id = id,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            IsActive = isActive
        };

        // Assert
        user.Id.Should().Be(id);
        user.Email.Should().Be(email);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Role.Should().Be(role);
        user.IsActive.Should().BeTrue();
        user.SiteUsers.Should().NotBeNull();
    }

    [Fact]
    public void User_Should_Support_Different_Roles()
    {
        // Arrange & Act
        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User",
            Role = "Admin",
            IsActive = true
        };

        var editor = new User
        {
            Id = Guid.NewGuid(),
            Email = "editor@example.com",
            FirstName = "Editor",
            LastName = "User",
            Role = "Editor",
            IsActive = true
        };

        var viewer = new User
        {
            Id = Guid.NewGuid(),
            Email = "viewer@example.com",
            FirstName = "Viewer",
            LastName = "User",
            Role = "Viewer",
            IsActive = true
        };

        // Assert
        admin.Role.Should().Be("Admin");
        editor.Role.Should().Be("Editor");
        viewer.Role.Should().Be("Viewer");
    }

    [Fact]
    public void User_Should_Allow_Adding_Site_Associations()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "multi.site@example.com",
            FirstName = "Multi",
            LastName = "Site",
            Role = "Editor",
            IsActive = true
        };
        var siteUser = new SiteUser
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            UserId = user.Id.ToString(),
            Role = "SiteAdmin"
        };

        // Act
        user.SiteUsers.Add(siteUser);

        // Assert
        user.SiteUsers.Should().HaveCount(1);
        user.SiteUsers.First().Role.Should().Be("SiteAdmin");
    }

    [Fact]
    public void User_Should_Support_Multiple_Site_Associations()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "super.admin@example.com",
            FirstName = "Super",
            LastName = "Admin",
            Role = "Admin",
            IsActive = true
        };

        // Act
        user.SiteUsers.Add(new SiteUser { Id = Guid.NewGuid(), SiteId = Guid.NewGuid(), UserId = user.Id.ToString(), Role = "SiteAdmin" });
        user.SiteUsers.Add(new SiteUser { Id = Guid.NewGuid(), SiteId = Guid.NewGuid(), UserId = user.Id.ToString(), Role = "SiteEditor" });
        user.SiteUsers.Add(new SiteUser { Id = Guid.NewGuid(), SiteId = Guid.NewGuid(), UserId = user.Id.ToString(), Role = "SiteViewer" });

        // Assert
        user.SiteUsers.Should().HaveCount(3);
        user.SiteUsers.Select(su => su.Role).Should().Contain(new[] { "SiteAdmin", "SiteEditor", "SiteViewer" });
    }

    [Fact]
    public void User_Should_Track_Active_State()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "inactive@example.com",
            FirstName = "Inactive",
            LastName = "User",
            Role = "Viewer",
            IsActive = true
        };

        // Act
        user.IsActive = false;

        // Assert
        user.IsActive.Should().BeFalse();
    }
}
