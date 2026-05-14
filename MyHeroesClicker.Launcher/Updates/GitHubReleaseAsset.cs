using System.Text.Json.Serialization;

namespace MyHeroesClicker.Launcher.Updates;

public sealed class GitHubReleaseAsset
{
  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("browser_download_url")]
  public string BrowserDownloadUrl { get; init; } = string.Empty;
}
