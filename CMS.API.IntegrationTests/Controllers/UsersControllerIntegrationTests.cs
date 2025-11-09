using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using CMS.Application.DTOs;
using CMS.Application.Features.Users.Commands;

namespace CMS.API.IntegrationTests.Controllers;

public class UsersControllerIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;

    public UsersControllerIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllUsers_ReturnsOkStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();
        users.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateUser_ReturnsCreatedUser_WhenValidRequest()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = $"testuser-{Guid.NewGuid()}@example.com",
            Password = "Test123!@#",
            FirstName = "Test",
            LastName = "User",
            Roles = new List<string> { "Editor" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.Email.Should().Be(command.Email);
        user.FirstName.Should().Be("Test");
        user.LastName.Should().Be("User");
        user.Roles.Should().Contain("Editor");
    }

    [Fact]
    public async Task CreateUser_ReturnsBadRequest_WhenDuplicateEmail()
    {
        // Arrange
        var email = $"duplicate-{Guid.NewGuid()}@example.com";
        var command1 = new CreateUserCommand
        {
            Email = email,
            Password = "Test123!@#",
            FirstName = "First",
            LastName = "User"
        };

        var command2 = new CreateUserCommand
        {
            Email = email,
            Password = "Test456!@#",
            FirstName = "Second",
            LastName = "User"
        };

        // Act
        await _client.PostAsJsonAsync("/api/users", command1);
        var response = await _client.PostAsJsonAsync("/api/users", command2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUserById_ReturnsUser_WhenUserExists()
    {
        // Arrange
        var createCommand = new CreateUserCommand
        {
            Email = $"getuser-{Guid.NewGuid()}@example.com",
            Password = "Test123!@#",
            FirstName = "Get",
            LastName = "User"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/users", createCommand);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();

        // Act
        var response = await _client.GetAsync($"/api/users/{createdUser!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.Id.Should().Be(createdUser.Id);
        user.Email.Should().Be(createCommand.Email);
    }

    [Fact]
    public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUser_ReturnsUpdatedUser_WhenValidRequest()
    {
        // Arrange
        var createCommand = new CreateUserCommand
        {
            Email = $"updateuser-{Guid.NewGuid()}@example.com",
            Password = "Test123!@#",
            FirstName = "Original",
            LastName = "User"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/users", createCommand);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();

        var updateCommand = new UpdateUserCommand
        {
            Id = createdUser!.Id,
            Email = createdUser.Email,
            FirstName = "Updated",
            LastName = "Name",
            Roles = new List<string> { "Admin" }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/users/{createdUser.Id}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.FirstName.Should().Be("Updated");
        user.LastName.Should().Be("Name");
        user.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task UpdateUser_ReturnsBadRequest_WhenIdMismatch()
    {
        // Arrange
        var updateCommand = new UpdateUserCommand
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateCommand = new UpdateUserCommand
        {
            Id = userId,
            Email = "nonexistent@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/users/{userId}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_ReturnsNoContent_WhenUserExists()
    {
        // Arrange
        var createCommand = new CreateUserCommand
        {
            Email = $"deleteuser-{Guid.NewGuid()}@example.com",
            Password = "Test123!@#",
            FirstName = "Delete",
            LastName = "User"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/users", createCommand);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/users/{createdUser!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify user is deleted
        var getResponse = await _client.GetAsync($"/api/users/{createdUser.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
