using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;

namespace MyHeroesClicker.Launcher.Updates;

public sealed class ReleaseUpdateService
{
  private const string ReleasesUrl = "https://api.github.com/repos/Skarpelen/my-heroes-clicker/releases?per_page=30";
  private const string WindowsSetupAssetName = "MyHeroesClickerSetup-win-x64.exe";

  private readonly HttpClient _httpClient;

  public ReleaseUpdateService(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public static ReleaseVersion GetCurrentVersion()
  {
    var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;

    if (assemblyVersion is null)
    {
      return new ReleaseVersion(0, 0, 0);
    }

    return new ReleaseVersion(assemblyVersion.Major, assemblyVersion.Minor, assemblyVersion.Build);
  }

  public async Task<ReleaseUpdate?> CheckAsync(CancellationToken cancellationToken)
  {
    var assetName = GetUpdateAssetName();

    if (assetName is null)
    {
      return null;
    }

    using var request = new HttpRequestMessage(HttpMethod.Get, ReleasesUrl);
    request.Headers.UserAgent.ParseAdd("MyHeroesClicker.Launcher");
    request.Headers.Accept.ParseAdd("application/vnd.github+json");

    using var response = await _httpClient.SendAsync(request, cancellationToken);

    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
      return null;
    }

    response.EnsureSuccessStatusCode();

    var releases = await response.Content.ReadFromJsonAsync<IReadOnlyList<GitHubRelease>>(cancellationToken);
    var currentVersion = GetCurrentVersion();
    var releaseCandidates = releases?
      .Select(item => new ReleaseCandidate(
        item,
        ReleaseVersion.TryParse(item.TagName, out var version) ? version : null,
        item.Assets.FirstOrDefault(asset => string.Equals(asset.Name, assetName, StringComparison.OrdinalIgnoreCase))))
      .Where(item => item.Version is not null)
      .OrderByDescending(item => item.Version)
      .ToArray();
    var release = releaseCandidates?
      .Where(item => item.Asset is not null && !string.IsNullOrWhiteSpace(item.Asset.BrowserDownloadUrl))
      .FirstOrDefault();

    if (release is null || release.Version is null || release.Asset is null)
    {
      return null;
    }

    return new ReleaseUpdate(
      currentVersion,
      release.Version,
      release.Release.HtmlUrl,
      release.Asset.Name,
      release.Asset.BrowserDownloadUrl,
      BuildChangelog(releaseCandidates ?? [], currentVersion, release.Version));
  }

  public async Task ApplyAsync(
    ReleaseUpdate update,
    IProgress<ReleaseUpdateProgress>? progress,
    CancellationToken cancellationToken)
  {
    if (!OperatingSystem.IsWindows())
    {
      throw new PlatformNotSupportedException("Автообновление через установщик пока поддерживается только на Windows.");
    }

    var updateRoot = Path.Combine(Path.GetTempPath(), "MyHeroesClicker", "updates", update.LatestVersion.ToString());
    var setupPath = Path.Combine(updateRoot, update.AssetName);

    if (Directory.Exists(updateRoot))
    {
      Directory.Delete(updateRoot, recursive: true);
    }

    Directory.CreateDirectory(updateRoot);

    using var request = new HttpRequestMessage(HttpMethod.Get, update.AssetDownloadUrl);
    request.Headers.UserAgent.ParseAdd("MyHeroesClicker.Launcher");

    using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    response.EnsureSuccessStatusCode();

    var totalBytes = response.Content.Headers.ContentLength;
    progress?.Report(new ReleaseUpdateProgress(0, totalBytes));

    await using (var setupStream = await response.Content.ReadAsStreamAsync(cancellationToken))
    await using (var fileStream = File.Create(setupPath))
    {
      var buffer = new byte[81920];
      var bytesReceived = 0L;

      while (true)
      {
        var bytesRead = await setupStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);

        if (bytesRead == 0)
        {
          break;
        }

        await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

        bytesReceived += bytesRead;
        progress?.Report(new ReleaseUpdateProgress(bytesReceived, totalBytes));
      }
    }

    StartUpdateScript(setupPath);
  }

  private static void StartUpdateScript(string setupPath)
  {
    var launcherPath = Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, "My Heroes Clicker.exe");
    var scriptPath = Path.Combine(Path.GetTempPath(), "MyHeroesClicker", "updates", $"apply-{Guid.NewGuid():N}.ps1");
    var currentProcessId = Environment.ProcessId;
    var script = $$"""
      $ErrorActionPreference = 'Stop'
      $processId = {{currentProcessId}}
      $setup = '{{EscapePowerShellSingleQuotedString(setupPath)}}'
      $launcher = '{{EscapePowerShellSingleQuotedString(launcherPath)}}'

      Wait-Process -Id $processId -ErrorAction SilentlyContinue
      Start-Process -FilePath $setup -ArgumentList '/SP- /SILENT /NORESTART' -Wait
      if (Test-Path -LiteralPath $launcher) {
        Start-Process -FilePath $launcher
      }
      """;

    Directory.CreateDirectory(Path.GetDirectoryName(scriptPath)!);
    File.WriteAllText(scriptPath, script, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

    Process.Start(new ProcessStartInfo("powershell.exe", $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"")
    {
      UseShellExecute = false,
      CreateNoWindow = true,
      WindowStyle = ProcessWindowStyle.Hidden
    });
  }

  private static string EscapePowerShellSingleQuotedString(string value)
  {
    return value.Replace("'", "''", StringComparison.Ordinal);
  }

  private static string? GetUpdateAssetName()
  {
    return OperatingSystem.IsWindows()
      ? WindowsSetupAssetName
      : null;
  }

  private static string BuildChangelog(
    IEnumerable<ReleaseCandidate> releases,
    ReleaseVersion currentVersion,
    ReleaseVersion latestVersion)
  {
    var builder = new StringBuilder();
    var applicableReleases = releases
      .Where(item => item.Version is not null
                     && item.Version.CompareTo(currentVersion) > 0
                     && item.Version.CompareTo(latestVersion) <= 0)
      .OrderByDescending(item => item.Version);

    foreach (var release in applicableReleases)
    {
      if (release.Version is null)
      {
        continue;
      }

      if (builder.Length > 0)
      {
        builder.AppendLine();
        builder.AppendLine();
      }

      builder.AppendLine($"v{release.Version}");
      builder.AppendLine(string.IsNullOrWhiteSpace(release.Release.Body)
        ? "Changelog не указан."
        : release.Release.Body.Trim());
    }

    return builder.ToString();
  }

  private sealed record ReleaseCandidate(
    GitHubRelease Release,
    ReleaseVersion? Version,
    GitHubReleaseAsset? Asset);
}

public sealed record ReleaseUpdateProgress(long BytesReceived, long? TotalBytes);
