using System.Text.RegularExpressions;

namespace CMS.Application.Services;

/// <summary>
/// Service to sanitize HTML content and prevent XSS attacks
/// </summary>
public interface IHtmlSanitizerService
{
    string Sanitize(string html);
    string StripAllHtml(string html);
}

public class HtmlSanitizerService : IHtmlSanitizerService
{
    // Allowed HTML tags for content (whitelist approach)
    private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "br", "strong", "em", "u", "h1", "h2", "h3", "h4", "h5", "h6",
        "ul", "ol", "li", "a", "img", "blockquote", "code", "pre",
        "table", "thead", "tbody", "tr", "th", "td", "div", "span"
    };

    // Allowed attributes for specific tags
    private static readonly Dictionary<string, HashSet<string>> AllowedAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        { "a", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "href", "title", "target" } },
        { "img", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "src", "alt", "title", "width", "height" } },
        { "div", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class" } },
        { "span", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class" } },
        { "table", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class" } },
        { "td", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "colspan", "rowspan" } },
        { "th", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "colspan", "rowspan" } }
    };

    public string Sanitize(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        // Remove script tags and their content
        html = Regex.Replace(html, @"<script[^>]*>.*?</script>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        
        // Remove event handlers (onclick, onerror, etc.)
        html = Regex.Replace(html, @"\s*on\w+\s*=\s*[""'][^""']*[""']", string.Empty, RegexOptions.IgnoreCase);
        html = Regex.Replace(html, @"\s*on\w+\s*=\s*[^\s>]*", string.Empty, RegexOptions.IgnoreCase);
        
        // Remove javascript: protocol from hrefs and srcs
        html = Regex.Replace(html, @"javascript\s*:", string.Empty, RegexOptions.IgnoreCase);
        html = Regex.Replace(html, @"vbscript\s*:", string.Empty, RegexOptions.IgnoreCase);
        html = Regex.Replace(html, @"data\s*:", string.Empty, RegexOptions.IgnoreCase);
        
        // Remove style tags and their content
        html = Regex.Replace(html, @"<style[^>]*>.*?</style>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        
        // Remove iframe, object, embed tags
        html = Regex.Replace(html, @"<(iframe|object|embed|form)[^>]*>.*?</\1>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        html = Regex.Replace(html, @"<(iframe|object|embed|form)[^>]*/>", string.Empty, RegexOptions.IgnoreCase);
        
        // Remove disallowed tags but keep their content
        html = RemoveDisallowedTags(html);
        
        // Remove disallowed attributes
        html = RemoveDisallowedAttributes(html);
        
        // Encode remaining special characters in text nodes
        html = EncodeSpecialCharactersInText(html);

        return html;
    }

    public string StripAllHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        // Remove all HTML tags
        var stripped = Regex.Replace(html, @"<[^>]*>", string.Empty);
        
        // Decode HTML entities
        stripped = System.Net.WebUtility.HtmlDecode(stripped);
        
        return stripped.Trim();
    }

    private string RemoveDisallowedTags(string html)
    {
        var tagPattern = @"</?(\w+)[^>]*>";
        return Regex.Replace(html, tagPattern, match =>
        {
            var tagName = match.Groups[1].Value;
            return AllowedTags.Contains(tagName) ? match.Value : string.Empty;
        }, RegexOptions.IgnoreCase);
    }

    private string RemoveDisallowedAttributes(string html)
    {
        var tagPattern = @"<(\w+)([^>]*)>";
        return Regex.Replace(html, tagPattern, match =>
        {
            var tagName = match.Groups[1].Value;
            var attributes = match.Groups[2].Value;

            if (!AllowedTags.Contains(tagName))
                return string.Empty;

            if (string.IsNullOrWhiteSpace(attributes))
                return match.Value;

            // Parse and filter attributes
            var filteredAttributes = FilterAttributes(tagName, attributes);
            return $"<{tagName}{filteredAttributes}>";
        }, RegexOptions.IgnoreCase);
    }

    private string FilterAttributes(string tagName, string attributes)
    {
        if (!AllowedAttributes.TryGetValue(tagName, out var allowedAttrs))
            return string.Empty;

        var attrPattern = @"(\w+)\s*=\s*[""']([^""']*)[""']";
        var matches = Regex.Matches(attributes, attrPattern);
        
        var filtered = new List<string>();
        foreach (Match match in matches)
        {
            var attrName = match.Groups[1].Value;
            var attrValue = match.Groups[2].Value;

            if (allowedAttrs.Contains(attrName))
            {
                // Additional validation for href and src
                if (attrName.Equals("href", StringComparison.OrdinalIgnoreCase) || 
                    attrName.Equals("src", StringComparison.OrdinalIgnoreCase))
                {
                    if (IsValidUrl(attrValue))
                        filtered.Add($"{attrName}=\"{System.Net.WebUtility.HtmlEncode(attrValue)}\"");
                }
                else
                {
                    filtered.Add($"{attrName}=\"{System.Net.WebUtility.HtmlEncode(attrValue)}\"");
                }
            }
        }

        return filtered.Count > 0 ? " " + string.Join(" ", filtered) : string.Empty;
    }

    private bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        // Block dangerous protocols
        if (Regex.IsMatch(url, @"^\s*(javascript|vbscript|data):", RegexOptions.IgnoreCase))
            return false;

        // Allow relative URLs and safe protocols
        return url.StartsWith("/") || 
               url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
               url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
               url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase);
    }

    private string EncodeSpecialCharactersInText(string html)
    {
        // This is a simplified approach - in production consider using a proper HTML parser
        // For now, we ensure that any remaining < or > outside of tags are encoded
        var insideTag = false;
        var result = new System.Text.StringBuilder();
        
        foreach (var ch in html)
        {
            if (ch == '<')
            {
                insideTag = true;
                result.Append(ch);
            }
            else if (ch == '>')
            {
                insideTag = false;
                result.Append(ch);
            }
            else if (!insideTag && (ch == '&' && !IsPartOfEntity(html, result.Length)))
            {
                result.Append("&amp;");
            }
            else
            {
                result.Append(ch);
            }
        }

        return result.ToString();
    }

    private bool IsPartOfEntity(string html, int position)
    {
        // Check if & is part of an HTML entity like &nbsp; or &#123;
        if (position >= html.Length - 1)
            return false;

        var entityPattern = @"&(#?\w+);";
        var substring = html.Substring(position, Math.Min(10, html.Length - position));
        return Regex.IsMatch(substring, entityPattern);
    }
}
