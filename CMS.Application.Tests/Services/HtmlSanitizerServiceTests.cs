using Xunit;
using CMS.Application.Services;

namespace CMS.Application.Tests.Services;

public class HtmlSanitizerServiceTests
{
    private readonly HtmlSanitizerService _sanitizer;

    public HtmlSanitizerServiceTests()
    {
        _sanitizer = new HtmlSanitizerService();
    }

    [Fact]
    public void Sanitize_ShouldRemoveScriptTags()
    {
        // Arrange
        var html = "<p>Hello</p><script>alert('XSS')</script><p>World</p>";

        // Act
        var result = _sanitizer.Sanitize(html);

        // Assert
        Assert.DoesNotContain("<script>", result);
        Assert.DoesNotContain("alert('XSS')", result);
        Assert.Contains("<p>Hello</p>", result);
        Assert.Contains("<p>World</p>", result);
    }

    [Fact]
    public void Sanitize_ShouldRemoveEventHandlers()
    {
        // Arrange
        var html = "<div onclick=\"alert('XSS')\">Click me</div>";

        // Act
        var result = _sanitizer.Sanitize(html);

        // Assert
        Assert.DoesNotContain("onclick", result);
        Assert.DoesNotContain("alert('XSS')", result);
    }

    [Fact]
    public void Sanitize_ShouldRemoveJavaScriptProtocol()
    {
        // Arrange
        var html = "<a href=\"javascript:alert('XSS')\">Click</a>";

        // Act
        var result = _sanitizer.Sanitize(html);

        // Assert
        Assert.DoesNotContain("javascript:", result);
    }

    [Fact]
    public void Sanitize_ShouldRemoveVBScriptProtocol()
    {
        // Arrange
        var html = "<a href=\"vbscript:msgbox('XSS')\">Click</a>";

        // Act
        var result = _sanitizer.Sanitize(html);

        // Assert
        Assert.DoesNotContain("vbscript:", result);
    }

    [Fact]
    public void Sanitize_ShouldRemoveDataProtocol()
    {
        // Arrange
        var html = "<a href=\"data:text/html,<script>alert('XSS')</script>\">Click</a>";

        // Act
        var result = _sanitizer.Sanitize(html);

        // Assert
        Assert.DoesNotContain("data:", result);
    }

    [Fact]
    public void Sanitize_ShouldRemoveIframeTags()
    {
        // Arrange
        var html = "<p>Safe content</p><iframe src=\"evil.com\"></iframe>";

        // Act
        var result = _sanitizer.Sanitize(html);

        // Assert
        Assert.DoesNotContain("<iframe", result);
        Assert.Contains("<p>Safe content</p>", result);
    }

    [Fact]
    public void Sanitize_ShouldRemoveObjectTags()
    {
        // Arrange
        var html = "<p>Safe</p><object data=\"evil.swf\"></object>";

        // Act
        var result = _sanitizer.Sanitize(html);

        // Assert
        Assert.DoesNotContain("<object", result);
        Assert.Contains("<p>Safe</p>", result);
    }

    [Fact]
    public void Sanitize_ShouldRemoveEmbedTags()
    {
        // Arrange
        var html = "<p>Safe</p><embed src=\"evil.swf\">";

        // Act
        var result = _sanitizer.Sanitize(html);

        // Assert
        Assert.DoesNotContain("<embed", result);
        Assert.Contains("<p>Safe</p>", result);
    }

    [Fact]
    public void Sanitize_ShouldRemoveFormTags()
    {
        // Arrange
        var html = "<form action=\"evil.com\"><input type=\"text\"></form>";

        // Act
        var result = _sanitizer.Sanitize(html);

        // Assert
        Assert.DoesNotContain("<form", result);
    }

    [Fact]
    public void Sanitize_ShouldRemoveStyleTags()
    {
        // Arrange
        var html = "<style>body { background: url('evil.com'); }</style><p>Content</p>";

        // Act
        var result = _sanitizer.Sanitize(html);

        // Assert
        Assert.DoesNotContain("<style>", result);
        Assert.Contains("<p>Content</p>", result);
    }

    [Fact]
    public void Sanitize_ShouldAllowSafeTags()
    {
        // Arrange
        var html = @"
            <h1>Title</h1>
            <p>Paragraph with <strong>bold</strong> and <em>italic</em></p>
            <ul>
                <li>Item 1</li>
                <li>Item 2</li>
            </ul>
            <a href=""https://example.com"" title=""Link"">Link</a>
            <img src=""image.jpg"" alt=""Image"">
        ";

        // Act
        var result = _sanitizer.Sanitize(html);

        // Assert
        Assert.Contains("<h1>", result);
        Assert.Contains("<p>", result);
        Assert.Contains("<strong>", result);
        Assert.Contains("<em>", result);
        Assert.Contains("<ul>", result);
        Assert.Contains("<li>", result);
        Assert.Contains("<a ", result);
        Assert.Contains("<img ", result);
    }

    [Fact]
    public void Sanitize_ShouldHandleNullInput()
    {
        // Act
        var result = _sanitizer.Sanitize(null!);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Sanitize_ShouldHandleEmptyString()
    {
        // Act
        var result = _sanitizer.Sanitize(string.Empty);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void StripAllHtml_ShouldRemoveAllTags()
    {
        // Arrange
        var html = "<p>Hello <strong>World</strong></p>";

        // Act
        var result = _sanitizer.StripAllHtml(html);

        // Assert
        Assert.DoesNotContain("<", result);
        Assert.DoesNotContain(">", result);
        Assert.Contains("Hello", result);
        Assert.Contains("World", result);
    }

    [Fact]
    public void Sanitize_ShouldEncodeSpecialCharacters()
    {
        // Arrange
        var html = "<p>Test &amp; Test</p>";

        // Act
        var result = _sanitizer.Sanitize(html);

        // Assert
        Assert.Contains("&amp;", result);
    }

    [Theory]
    [InlineData("onload")]
    [InlineData("onerror")]
    [InlineData("onmouseover")]
    [InlineData("onclick")]
    [InlineData("onfocus")]
    public void Sanitize_ShouldRemoveVariousEventHandlers(string eventHandler)
    {
        // Arrange
        var html = $"<img {eventHandler}=\"alert('XSS')\" src=\"test.jpg\">";

        // Act
        var result = _sanitizer.Sanitize(html);

        // Assert
        Assert.DoesNotContain(eventHandler, result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sanitize_ShouldValidateUrls_AndRemoveInvalidOnes()
    {
        // Arrange
        var html = "<a href=\"not-a-valid-url\">Link</a>";

        // Act
        var result = _sanitizer.Sanitize(html);

        // Assert - Invalid URLs should be removed or the attribute stripped
        Assert.DoesNotContain("href=\"not-a-valid-url\"", result);
    }

    [Fact]
    public void Sanitize_ShouldPreserveValidHttpsUrls()
    {
        // Arrange
        var html = "<a href=\"https://example.com\">Link</a>";

        // Act
        var result = _sanitizer.Sanitize(html);

        // Assert
        Assert.Contains("https://example.com", result);
    }
}
