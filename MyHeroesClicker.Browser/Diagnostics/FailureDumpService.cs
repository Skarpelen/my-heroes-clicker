using Microsoft.Playwright;

namespace MyHeroesClicker.Browser.Diagnostics;

public sealed class FailureDumpService
{
  public async Task SaveAsync(IPage page, string reason, CancellationToken cancellationToken)
  {
    var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    var directory = Path.Combine("failures", timestamp);

    Directory.CreateDirectory(directory);

    await File.WriteAllTextAsync(
      Path.Combine(directory, "reason.txt"),
      reason,
      cancellationToken);

    await File.WriteAllTextAsync(
      Path.Combine(directory, "url.txt"),
      page.Url,
      cancellationToken);

    var html = await page.ContentAsync();

    await File.WriteAllTextAsync(
      Path.Combine(directory, "page.html"),
      html,
      cancellationToken);

    await page.ScreenshotAsync(new()
    {
      Path = Path.Combine(directory, "screenshot.png"),
      FullPage = true
    });
  }
}
