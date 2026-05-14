using System.Text.Json.Serialization;

namespace MyHeroesClicker.Launcher.Updates;

public sealed class GitHubRelease
{
  [JsonPropertyName("tag_name")]
  public string TagName { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("body")]
  public string? Body { get; init; }

  [JsonPropertyName("html_url")]
  public string HtmlUrl { get; init; } = string.Empty;

  [JsonPropertyName("assets")]
  public IReadOnlyList<GitHubReleaseAsset> Assets { get; init; } = [];
}
