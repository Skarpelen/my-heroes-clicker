using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;

namespace MyHeroesClicker.Launcher.Updates;

public sealed class ReleaseUpdateService
{
  private const string ReleasesUrl = "https://api.github.com/repos/Skarpelen/my-heroes-clicker/releases?per_page=30";
  private const string AssetName = "my-heroes-clicker-win-x64.zip";

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
    var release = releases?
      .Select(item => new
      {
        Release = item,
        Version = ReleaseVersion.TryParse(item.TagName, out var version) ? version : null,
        Asset = item.Assets.FirstOrDefault(asset => string.Equals(asset.Name, AssetName, StringComparison.OrdinalIgnoreCase))
      })
      .Where(item => item.Version is not null && item.Asset is not null && !string.IsNullOrWhiteSpace(item.Asset.BrowserDownloadUrl))
      .OrderByDescending(item => item.Version)
      .FirstOrDefault();

    if (release is null || release.Version is null || release.Asset is null)
    {
      return null;
    }

    return new ReleaseUpdate(
      GetCurrentVersion(),
      release.Version,
      release.Release.HtmlUrl,
      release.Asset.Name,
      release.Asset.BrowserDownloadUrl,
      release.Release.Body ?? string.Empty);
  }

  public async Task ApplyAsync(ReleaseUpdate update, CancellationToken cancellationToken)
  {
    var updateRoot = Path.Combine(Path.GetTempPath(), "MyHeroesClicker", "updates", update.LatestVersion.ToString());
    var archivePath = Path.Combine(updateRoot, update.AssetName);
    var extractPath = Path.Combine(updateRoot, "package");

    if (Directory.Exists(updateRoot))
    {
      Directory.Delete(updateRoot, recursive: true);
    }

    Directory.CreateDirectory(updateRoot);

    await using (var archiveStream = await _httpClient.GetStreamAsync(update.AssetDownloadUrl, cancellationToken))
    await using (var fileStream = File.Create(archivePath))
    {
      await archiveStream.CopyToAsync(fileStream, cancellationToken);
    }

    ZipFile.ExtractToDirectory(archivePath, extractPath);

    StartUpdateScript(extractPath);
  }

  private static void StartUpdateScript(string sourceDirectory)
  {
    var targetDirectory = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    var launcherPath = Environment.ProcessPath ?? Path.Combine(targetDirectory, "MyHeroesClicker.Launcher.exe");
    var scriptPath = Path.Combine(Path.GetTempPath(), "MyHeroesClicker", "updates", $"apply-{Guid.NewGuid():N}.ps1");
    var currentProcessId = Environment.ProcessId;
    var script = $$"""
      $ErrorActionPreference = 'Stop'
      $processId = {{currentProcessId}}
      $source = '{{EscapePowerShellSingleQuotedString(sourceDirectory)}}'
      $target = '{{EscapePowerShellSingleQuotedString(targetDirectory)}}'
      $launcher = '{{EscapePowerShellSingleQuotedString(launcherPath)}}'

      Wait-Process -Id $processId -ErrorAction SilentlyContinue
      Get-ChildItem -LiteralPath $source -Force | Copy-Item -Destination $target -Recurse -Force
      Start-Process -FilePath $launcher -WorkingDirectory $target
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
}
