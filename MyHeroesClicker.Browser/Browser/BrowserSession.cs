using Microsoft.Playwright;
using MyHeroesClicker.Core.Confs;

namespace MyHeroesClicker.Browser.Browser;

public sealed class BrowserSession : IAsyncDisposable
{
  private readonly IBrowserContext _context;

  private BrowserSession(IBrowserContext context, IPage page)
  {
    _context = context;
    Page = page;
  }

  public IPage Page { get; }

  public IBrowserContext Context => _context;

  public static async Task<BrowserSession> StartAsync(IPlaywright playwright, ClickerOptions options)
  {
    var userDataDir = Path.GetFullPath(options.UserDataDir);

    Directory.CreateDirectory(userDataDir);

    var browserKind = options.BrowserKind.Trim().ToLowerInvariant();
    var browserType = GetBrowserType(playwright, browserKind, out var channel);
    var launchOptions = CreateLaunchOptions(options, browserKind, channel);
    var context = await browserType.LaunchPersistentContextAsync(userDataDir, launchOptions);

    context.SetDefaultTimeout(options.DefaultTimeoutMs);
    context.SetDefaultNavigationTimeout(options.DefaultTimeoutMs);

    var page = context.Pages.Count > 0
      ? context.Pages[0]
      : await context.NewPageAsync();

    return new BrowserSession(context, page);
  }

  public async ValueTask DisposeAsync()
  {
    await _context.DisposeAsync();
  }

  private static BrowserTypeLaunchPersistentContextOptions CreateLaunchOptions(
    ClickerOptions options,
    string browserKind,
    string? channel)
  {
    var launchOptions = new BrowserTypeLaunchPersistentContextOptions
    {
      Channel = channel,
      Headless = options.Headless,
      SlowMo = 50,
      Locale = "ru-RU",
      ViewportSize = new()
      {
        Width = 1280,
        Height = 900
      }
    };

    if (IsChromiumBrowser(browserKind))
    {
      launchOptions.Args = ["--disable-blink-features=AutomationControlled"];
      launchOptions.IgnoreDefaultArgs = ["--enable-automation"];
    }

    return launchOptions;
  }

  private static IBrowserType GetBrowserType(IPlaywright playwright, string browserKind, out string? channel)
  {
    channel = null;

    return browserKind switch
    {
      "chromium" => playwright.Chromium,
      "chrome" => GetChromiumChannel(playwright, "chrome", out channel),
      "edge" => GetChromiumChannel(playwright, "msedge", out channel),
      "firefox" => playwright.Firefox,
      "webkit" => playwright.Webkit,
      _ => throw new InvalidOperationException($"Неподдерживаемый браузер: {browserKind}.")
    };
  }

  private static bool IsChromiumBrowser(string browserKind)
  {
    return browserKind is "chromium" or "chrome" or "edge";
  }

  private static IBrowserType GetChromiumChannel(IPlaywright playwright, string channelName, out string channel)
  {
    channel = channelName;

    return playwright.Chromium;
  }
}
