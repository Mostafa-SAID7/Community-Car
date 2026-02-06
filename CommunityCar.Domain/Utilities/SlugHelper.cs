using System;
using System.Text.RegularExpressions;

namespace CommunityCar.Domain.Utilities;

public static class SlugHelper
{
    public static string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Guid.NewGuid().ToString("N")[..8];

        var slug = title.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace(":", "")
            .Replace(";", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("{", "")
            .Replace("}", "")
            .Replace("/", "-")
            .Replace("\\", "-")
            .Replace("&", "and");

        // Remove multiple consecutive dashes
        while (slug.Contains("--"))
            slug = slug.Replace("--", "-");

        // Remove leading and trailing dashes
        slug = slug.Trim('-');

        // Ensure slug is not empty and not too long
        if (string.IsNullOrWhiteSpace(slug))
            slug = Guid.NewGuid().ToString("N")[..8];
        else if (slug.Length > 100)
            slug = slug[..100].TrimEnd('-');

        return slug;
    }
}
