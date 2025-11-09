using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using CMS.Application.DTOs;
using CMS.Application.Features.Sites.Commands;

namespace CMS.API.IntegrationTests.Controllers;

public class MediaControllerIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;

    public MediaControllerIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateTestSiteAsync()
    {
        var command = new CreateSiteCommand
        {
            Name = $"Media Test Site {Guid.NewGuid()}",
            Domain = $"mediatest-{Guid.NewGuid()}.example.com"
        };
        var response = await _client.PostAsJsonAsync("/api/sites", command);
        var site = await response.Content.ReadFromJsonAsync<SiteDto>();
        return site!.Id;
    }

    [Fact]
    public async Task UploadMedia_ReturnsOk_WhenValidFile()
    {
        // Arrange
        var siteId = await CreateTestSiteAsync();
        
        var fileContent = Encoding.UTF8.GetBytes("Fake image content");
        var content = new MultipartFormDataContent();
        var fileContent2 = new ByteArrayContent(fileContent);
        fileContent2.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        content.Add(fileContent2, "File", "test-image.png");
        content.Add(new StringContent(siteId.ToString()), "SiteId");
        content.Add(new StringContent("Test image description"), "Description");

        // Act
        var response = await _client.PostAsync("/api/media/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        result.Should().NotBeNull();
        result.Should().ContainKey("id");
        result.Should().ContainKey("message");
    }

    [Fact]
    public async Task UploadMedia_ReturnsBadRequest_WhenNoFile()
    {
        // Arrange
        var siteId = await CreateTestSiteAsync();
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(siteId.ToString()), "SiteId");

        // Act
        var response = await _client.PostAsync("/api/media/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetMediaBySite_ReturnsOk_WhenSiteExists()
    {
        // Arrange
        var siteId = await CreateTestSiteAsync();

        // Act
        var response = await _client.GetAsync($"/api/media/site/{siteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var media = await response.Content.ReadFromJsonAsync<IEnumerable<MediaDto>>();
        media.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMediaBySite_ReturnsEmptyList_WhenNoMediaExists()
    {
        // Arrange
        var siteId = await CreateTestSiteAsync();

        // Act
        var response = await _client.GetAsync($"/api/media/site/{siteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var media = await response.Content.ReadFromJsonAsync<IEnumerable<MediaDto>>();
        media.Should().NotBeNull();
        media.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteMedia_ReturnsNoContent_WhenMediaExists()
    {
        // Arrange
        var siteId = await CreateTestSiteAsync();
        
        // Upload a file first
        var fileContent = Encoding.UTF8.GetBytes("Fake image content");
        var uploadContent = new MultipartFormDataContent();
        var fileContent2 = new ByteArrayContent(fileContent);
        fileContent2.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        uploadContent.Add(fileContent2, "File", "delete-test.png");
        uploadContent.Add(new StringContent(siteId.ToString()), "SiteId");
        
        var uploadResponse = await _client.PostAsync("/api/media/upload", uploadContent);
        var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var mediaId = uploadResult!["id"].ToString();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/media/{mediaId}?mediaType=Image");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteMedia_ReturnsNotFound_WhenMediaDoesNotExist()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/media/{Guid.NewGuid()}?mediaType=Image");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UploadMultipleFiles_AllSucceed()
    {
        // Arrange
        var siteId = await CreateTestSiteAsync();
        
        // Upload first file
        var fileContent1 = Encoding.UTF8.GetBytes("First image");
        var content1 = new MultipartFormDataContent();
        var fileByteContent1 = new ByteArrayContent(fileContent1);
        fileByteContent1.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content1.Add(fileByteContent1, "File", "image1.jpg");
        content1.Add(new StringContent(siteId.ToString()), "SiteId");
        
        // Upload second file
        var fileContent2 = Encoding.UTF8.GetBytes("Second image");
        var content2 = new MultipartFormDataContent();
        var fileByteContent2 = new ByteArrayContent(fileContent2);
        fileByteContent2.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content2.Add(fileByteContent2, "File", "image2.jpg");
        content2.Add(new StringContent(siteId.ToString()), "SiteId");

        // Act
        var response1 = await _client.PostAsync("/api/media/upload", content1);
        var response2 = await _client.PostAsync("/api/media/upload", content2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify both files exist
        var getResponse = await _client.GetAsync($"/api/media/site/{siteId}");
        var media = await getResponse.Content.ReadFromJsonAsync<IEnumerable<MediaDto>>();
        media.Should().NotBeNull();
        media.Should().HaveCountGreaterOrEqualTo(2);
    }
}
