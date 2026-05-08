using Microsoft.Playwright;
using MyHeroesClicker.Core.Confs;

namespace MyHeroesClicker.Browser.Browser;

public sealed class BrowserSession : IAsyncDisposable
{
  private readonly IBrowserContext _context;
  private readonly string _authStatePath;

  private BrowserSession(IBrowserContext context, IPage page, string authStatePath)
  {
    _context = context;
    _authStatePath = authStatePath;
    Page = page;
  }

  public IPage Page { get; }

  public IBrowserContext Context => _context;

  public static async Task<BrowserSession> StartAsync(IPlaywright playwright, ClickerOptions options)
  {
    var userDataDir = Path.GetFullPath(options.UserDataDir);
    var authStatePath = Path.GetFullPath(options.AuthStatePath);

    Directory.CreateDirectory(userDataDir);
    Directory.CreateDirectory(Path.GetDirectoryName(authStatePath)!);

    var context = await playwright.Chromium.LaunchPersistentContextAsync(userDataDir, new()
    {
      Channel = "chrome",
      Headless = options.Headless,
      SlowMo = 50,
      Locale = "ru-RU",
      ViewportSize = new()
      {
        Width = 1280,
        Height = 900
      }
    });

    if (File.Exists(authStatePath))
    {
      await context.SetStorageStateAsync(authStatePath);
    }

    context.SetDefaultTimeout(options.DefaultTimeoutMs);
    context.SetDefaultNavigationTimeout(options.DefaultTimeoutMs);

    var page = context.Pages.Count > 0
      ? context.Pages[0]
      : await context.NewPageAsync();

    return new BrowserSession(context, page, authStatePath);
  }

  public async Task SaveAuthStateAsync()
  {
    await _context.StorageStateAsync(new()
    {
      Path = _authStatePath,
      IndexedDB = true
    });
  }

  public async ValueTask DisposeAsync()
  {
    await _context.DisposeAsync();
  }
}
