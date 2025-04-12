using System;
using System.Text.Json.Serialization;

namespace wizbotupdater.Services;

/// <summary>
/// Model representing a GitHub release.
/// </summary>
public class ReleaseModel
{
    /// <summary>
    /// The name of the release.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The tag name of the release (e.g., "v1.0.0").
    /// </summary>
    [JsonPropertyName("tag_name")]
    public string? TagName { get; set; }

    /// <summary>
    /// The URL to the release page.
    /// </summary>
    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }

    /// <summary>
    /// The release description/body.
    /// </summary>
    [JsonPropertyName("body")]
    public string? Body { get; set; }

    /// <summary>
    /// Whether this is a prerelease.
    /// </summary>
    [JsonPropertyName("prerelease")]
    public bool IsPrerelease { get; set; }

    /// <summary>
    /// The published date of the release.
    /// </summary>
    [JsonPropertyName("published_at")]
    public DateTime PublishedAt { get; set; }

    /// <summary>
    /// The assets (downloadable files) of the release.
    /// </summary>
    [JsonPropertyName("assets")]
    public ReleaseAsset[]? Assets { get; set; }
}