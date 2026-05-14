namespace MyHeroesClicker.Launcher.Updates;

public sealed record ReleaseUpdate(
  ReleaseVersion CurrentVersion,
  ReleaseVersion LatestVersion,
  string ReleaseUrl,
  string AssetName,
  string AssetDownloadUrl,
  string Changelog)
{
  public bool IsUpdateAvailable => LatestVersion.CompareTo(CurrentVersion) > 0;

  public bool IsNewVersionLine => LatestVersion.IsNewVersionLineComparedTo(CurrentVersion);
}
