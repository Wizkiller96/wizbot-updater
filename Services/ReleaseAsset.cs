using System.Text.Json.Serialization;

namespace wizbotupdater.Services;

/// <summary>
/// Model representing a GitHub release asset (downloadable file).
/// </summary>
public class ReleaseAsset
{
    /// <summary>
    /// The name of the asset.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The URL to download the asset.
    /// </summary>
    [JsonPropertyName("browser_download_url")]
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// The size of the asset in bytes.
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; set; }
}