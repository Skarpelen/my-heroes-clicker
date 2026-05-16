using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Threading;
using MyHeroesClicker.Launcher.Updates;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MyHeroesClicker.Launcher;

public partial class MainWindow : Window
{
  private readonly HttpClient _httpClient = new()
  {
    Timeout = TimeSpan.FromSeconds(2)
  };

  private readonly HttpClient _updateHttpClient = new()
  {
    Timeout = TimeSpan.FromMinutes(5)
  };

  private readonly TextBlock _versionTextBlock;
  private readonly Border _betaBadge;
  private readonly Button _updateAlertButton;
  private readonly Button _updateNewVersionButton;
  private readonly Border _updateDetailsPanel;
  private readonly TextBlock _updateDetailsTextBlock;
  private readonly TextBlock _errorTextBlock;
  private readonly Border _diagnosticsPanel;
  private readonly TextBox _changelogTextBox;
  private readonly TextBox _diagnosticsTextBox;
  private readonly Button _startButton;
  private readonly Button _openBrowserButton;
  private readonly Button _installUpdateButton;
  private readonly NativeWebView _browser;
  private readonly ReleaseUpdateService _releaseUpdateService;
  private readonly CancellationTokenSource _updateCheckCancellation = new();
  private readonly string _logFilePath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "MyHeroesClicker",
    "logs",
    "launcher.log");

  private Process? _backendProcess;
  private Uri? _applicationUri;
  private ReleaseUpdate? _availableUpdate;
  private bool _isStarting;
  private bool _isCheckingUpdates;
  private bool _isInstallingUpdate;

  public MainWindow()
  {
    InitializeComponent();

    _versionTextBlock = GetRequiredControl<TextBlock>("VersionTextBlock");
    _betaBadge = GetRequiredControl<Border>("BetaBadge");
    _updateAlertButton = GetRequiredControl<Button>("UpdateAlertButton");
    _updateNewVersionButton = GetRequiredControl<Button>("UpdateNewVersionButton");
    _updateDetailsPanel = GetRequiredControl<Border>("UpdateDetailsPanel");
    _updateDetailsTextBlock = GetRequiredControl<TextBlock>("UpdateDetailsTextBlock");
    _errorTextBlock = GetRequiredControl<TextBlock>("ErrorTextBlock");
    _diagnosticsPanel = GetRequiredControl<Border>("DiagnosticsPanel");
    _changelogTextBox = GetRequiredControl<TextBox>("ChangelogTextBox");
    _diagnosticsTextBox = GetRequiredControl<TextBox>("DiagnosticsTextBox");
    _startButton = GetRequiredControl<Button>("StartButton");
    _openBrowserButton = GetRequiredControl<Button>("OpenBrowserButton");
    _installUpdateButton = GetRequiredControl<Button>("InstallUpdateButton");
    _browser = GetRequiredControl<NativeWebView>("Browser");
    _browser.EnvironmentRequested += Browser_OnEnvironmentRequested;
    _releaseUpdateService = new ReleaseUpdateService(_updateHttpClient);

    var version = ReleaseUpdateService.GetCurrentVersion();
    _versionTextBlock.Text = $"v{version}";
    _betaBadge.IsVisible = version.Major == 0;
    SetStatus("Лаунчер готов к запуску.");
    AppendDiagnostic($"Лог лаунчера: {_logFilePath}");

    Loaded += MainWindow_OnLoaded;
    Closing += MainWindow_OnClosing;
  }

  private static void Browser_OnEnvironmentRequested(object? sender, WebViewEnvironmentRequestedEventArgs args)
  {
    if (args is WindowsWebView2EnvironmentRequestedEventArgs webView2)
    {
      var userDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MyHeroesClicker",
        "WebView2");

      Directory.CreateDirectory(userDataFolder);

      webView2.UserDataFolder = userDataFolder;
    }
  }

  private T GetRequiredControl<T>(string name)
    where T : Control
  {
    return this.FindControl<T>(name)
      ?? throw new InvalidOperationException($"Control '{name}' was not found.");
  }

  private async void MainWindow_OnLoaded(object? sender, RoutedEventArgs e)
  {
    await CheckUpdatesAsync(showUpToDateMessage: false);
    _ = CheckUpdatesPeriodicallyAsync(_updateCheckCancellation.Token);
  }

  private async void StartButton_OnClick(object? sender, RoutedEventArgs e)
  {
    if (_isStarting)
    {
      return;
    }

    await StartApplicationAsync();
  }

  private async Task StartApplicationAsync()
  {
    if (_backendProcess is not null && _backendProcess.HasExited)
    {
      _backendProcess.Dispose();
      _backendProcess = null;
    }

    if (_backendProcess is not null && _applicationUri is not null)
    {
      _openBrowserButton.IsEnabled = true;
      SetStatus($"Backend уже запущен: {_applicationUri}");
      OpenInterface();

      return;
    }

    _isStarting = true;
    _startButton.IsEnabled = false;
    _openBrowserButton.IsEnabled = false;
    _errorTextBlock.Text = string.Empty;

    try
    {
      var port = GetFreeTcpPort();
      _applicationUri = new Uri($"http://127.0.0.1:{port}");

      SetStatus($"Запуск backend на порту {port}...");
      _backendProcess = StartBackendProcess(port);

      SetStatus("Ожидание готовности backend...");
      await WaitForBackendAsync(_applicationUri);

      _openBrowserButton.IsEnabled = true;
      _startButton.Content = "Запущено";
      SetStatus($"Backend готов: {_applicationUri}");

      OpenInterface();
    }
    catch (Exception exception)
    {
      _errorTextBlock.Text = exception.Message;
      _diagnosticsPanel.IsVisible = true;
      SetStatus("Запуск остановлен из-за ошибки.");
      StopBackend();
    }
    finally
    {
      _startButton.IsEnabled = _backendProcess is null || _backendProcess.HasExited;
      _isStarting = false;
    }
  }

  private Process StartBackendProcess(int port)
  {
    var executable = ResolveExecutable("MyHeroesClicker.API");
    var startInfo = CreateHiddenProcessStartInfo(executable.FileName, executable.Arguments);

    startInfo.Environment["ASPNETCORE_URLS"] = $"http://127.0.0.1:{port}";
    startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";

    var process = Process.Start(startInfo);

    if (process is null)
    {
      throw new InvalidOperationException("Не удалось запустить backend.");
    }

    process.OutputDataReceived += (_, args) => AppendDiagnosticFromProcess("API", args.Data);
    process.ErrorDataReceived += (_, args) => AppendDiagnosticFromProcess("API", args.Data);
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    return process;
  }

  private static ProcessStartInfo CreateHiddenProcessStartInfo(string fileName, string arguments)
  {
    var startInfo = new ProcessStartInfo(fileName, arguments)
    {
      UseShellExecute = false,
      CreateNoWindow = true,
      RedirectStandardError = true,
      RedirectStandardOutput = true,
      StandardErrorEncoding = Encoding.UTF8,
      StandardOutputEncoding = Encoding.UTF8
    };

    if (OperatingSystem.IsWindows())
    {
      startInfo.WindowStyle = ProcessWindowStyle.Hidden;
    }

    return startInfo;
  }

  private async Task WaitForBackendAsync(Uri applicationUri)
  {
    var healthUri = new Uri(applicationUri, "/api/health");
    var deadline = DateTimeOffset.UtcNow.AddSeconds(30);

    while (DateTimeOffset.UtcNow < deadline)
    {
      if (_backendProcess is not null && _backendProcess.HasExited)
      {
        throw new InvalidOperationException($"Backend завершился до готовности. Код завершения: {_backendProcess.ExitCode}.");
      }

      try
      {
        using var response = await _httpClient.GetAsync(healthUri);

        if (response.IsSuccessStatusCode)
        {
          return;
        }
      }
      catch (HttpRequestException)
      {
      }
      catch (TaskCanceledException)
      {
      }

      await Task.Delay(500);
    }

    throw new TimeoutException("Backend не ответил на health-check за 30 секунд.");
  }

  private void OpenInterface()
  {
    if (_applicationUri is null)
    {
      return;
    }

    try
    {
      _browser.Source = _applicationUri;
    }
    catch (Exception exception)
    {
      _errorTextBlock.Text = $"Не удалось открыть встроенный интерфейс. Используйте внешний браузер. {exception.Message}";
      _diagnosticsPanel.IsVisible = true;
      AppendDiagnostic(_errorTextBlock.Text);
    }
  }

  private void OpenBrowserButton_OnClick(object? sender, RoutedEventArgs e)
  {
    if (_applicationUri is null)
    {
      return;
    }

    OpenExternalUri(_applicationUri);
  }

  private void UpdateAlertButton_OnClick(object? sender, RoutedEventArgs e)
  {
    _updateDetailsPanel.IsVisible = !_updateDetailsPanel.IsVisible;
  }

  private void DiagnosticsButton_OnClick(object? sender, RoutedEventArgs e)
  {
    _diagnosticsPanel.IsVisible = !_diagnosticsPanel.IsVisible;
  }

  private void Browser_OnNavigationStarted(object? sender, WebViewNavigationStartingEventArgs e)
  {
    var request = e.Request;

    if (request is null || !ShouldOpenExternally(request))
    {
      return;
    }

    e.Cancel = true;
    OpenExternalUri(request);
  }

  private void Browser_OnNewWindowRequested(object? sender, WebViewNewWindowRequestedEventArgs e)
  {
    e.Handled = true;

    var request = e.Request;

    if (request is null)
    {
      return;
    }

    if (ShouldOpenExternally(request))
    {
      OpenExternalUri(request);
      return;
    }

    _browser.Source = request;
  }

  private async void InstallUpdateButton_OnClick(object? sender, RoutedEventArgs e)
  {
    if (_availableUpdate is null || _isInstallingUpdate)
    {
      return;
    }

    _isInstallingUpdate = true;
    _installUpdateButton.IsEnabled = false;
    _updateDetailsTextBlock.Text = $"Скачивание версии v{_availableUpdate.LatestVersion}...";

    try
    {
      await _releaseUpdateService.ApplyAsync(_availableUpdate, CancellationToken.None);
      _updateDetailsTextBlock.Text = "Обновление скачано. Лаунчер перезапустится.";
      StopBackend();
      ShutdownApplication();
    }
    catch (Exception exception)
    {
      _errorTextBlock.Text = $"Не удалось установить обновление. {exception.Message}";
      AppendDiagnostic(_errorTextBlock.Text);
      _installUpdateButton.IsEnabled = true;
      _isInstallingUpdate = false;
    }
  }

  private void MainWindow_OnClosing(object? sender, WindowClosingEventArgs e)
  {
    _updateCheckCancellation.Cancel();
    _updateCheckCancellation.Dispose();
    StopBackend();
    _httpClient.Dispose();
    _updateHttpClient.Dispose();
  }

  private async Task CheckUpdatesPeriodicallyAsync(CancellationToken cancellationToken)
  {
    using var timer = new PeriodicTimer(TimeSpan.FromHours(1));

    try
    {
      while (await timer.WaitForNextTickAsync(cancellationToken))
      {
        Dispatcher.UIThread.Post(async () => await CheckUpdatesAsync(showUpToDateMessage: false));
      }
    }
    catch (OperationCanceledException)
    {
    }
  }

  private async Task CheckUpdatesAsync(bool showUpToDateMessage)
  {
    if (_isCheckingUpdates)
    {
      return;
    }

    _isCheckingUpdates = true;
    AppendDiagnostic("Проверяю обновления...");

    try
    {
      var update = await _releaseUpdateService.CheckAsync(CancellationToken.None);
      _availableUpdate = update is { IsUpdateAvailable: true } ? update : null;
      RenderUpdateState(update, showUpToDateMessage);
    }
    catch (Exception exception)
    {
      _errorTextBlock.Text = "Не удалось проверить обновления.";
      AppendDiagnostic($"Проверка обновлений завершилась с ошибкой: {exception.Message}");
    }
    finally
    {
      _isCheckingUpdates = false;
    }
  }

  private void RenderUpdateState(ReleaseUpdate? update, bool showUpToDateMessage)
  {
    _updateAlertButton.IsVisible = false;
    _updateNewVersionButton.IsVisible = false;
    _updateDetailsPanel.IsVisible = false;
    _installUpdateButton.IsEnabled = false;
    _changelogTextBox.Text = string.Empty;
    _updateDetailsTextBlock.Text = string.Empty;

    if (update is null)
    {
      if (showUpToDateMessage)
      {
        AppendDiagnostic("Опубликованных релизов пока не найдено.");
      }

      return;
    }

    if (!update.IsUpdateAvailable)
    {
      if (showUpToDateMessage)
      {
        AppendDiagnostic($"Установлена актуальная версия v{update.CurrentVersion}.");
      }

      return;
    }

    if (update.IsNewVersionLine)
    {
      _updateNewVersionButton.IsVisible = true;
    }
    else
    {
      _updateAlertButton.IsVisible = true;
    }

    _installUpdateButton.IsEnabled = true;
    _updateDetailsTextBlock.Text = update.IsNewVersionLine
      ? $"Доступна версия v{update.LatestVersion}. Новая ветка версии, обновление рекомендуется."
      : $"Доступна версия v{update.LatestVersion}. Патч-обновление.";

    _changelogTextBox.Text = string.IsNullOrWhiteSpace(update.Changelog)
      ? "Changelog не указан."
      : update.Changelog.Trim();

    if (showUpToDateMessage)
    {
      _updateDetailsPanel.IsVisible = true;
    }
  }

  private void StopBackend()
  {
    if (_backendProcess is null)
    {
      return;
    }

    try
    {
      if (!_backendProcess.HasExited)
      {
        _backendProcess.Kill(entireProcessTree: true);
        _backendProcess.WaitForExit(5000);
      }
    }
    finally
    {
      _backendProcess.Dispose();
      _backendProcess = null;
    }
  }

  private void SetStatus(string message)
  {
    AppendDiagnostic(message);
  }

  private void AppendDiagnosticFromProcess(string source, string? message)
  {
    if (string.IsNullOrWhiteSpace(message))
    {
      return;
    }

    Dispatcher.UIThread.Post(() => AppendDiagnostic($"{source}: {message}"));
  }

  private void AppendDiagnostic(string message)
  {
    var line = $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss} {message}";
    var currentText = _diagnosticsTextBox.Text;
    _diagnosticsTextBox.Text = string.IsNullOrEmpty(currentText)
      ? line + Environment.NewLine
      : currentText + line + Environment.NewLine;
    _diagnosticsTextBox.CaretIndex = _diagnosticsTextBox.Text.Length;

    try
    {
      Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath)!);
      File.AppendAllText(_logFilePath, line + Environment.NewLine, Encoding.UTF8);
    }
    catch
    {
    }
  }

  private static int GetFreeTcpPort()
  {
    var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();

    try
    {
      return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
    finally
    {
      listener.Stop();
    }
  }

  private static ExecutableCommand ResolveExecutable(string projectName)
  {
    foreach (var candidate in EnumerateExecutableCandidates(projectName))
    {
      if (!File.Exists(candidate.ExecutablePath) || !IsRunnableExecutableCandidate(candidate.ExecutablePath))
      {
        continue;
      }

      if (candidate.ExecutablePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
      {
        return new ExecutableCommand("dotnet", $"\"{candidate.ExecutablePath}\"");
      }

      return new ExecutableCommand(candidate.ExecutablePath, string.Empty);
    }

    throw new FileNotFoundException($"Не найден исполняемый файл проекта {projectName}.");
  }

  private static bool IsRunnableExecutableCandidate(string executablePath)
  {
    if (executablePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
    {
      return true;
    }

    var directory = Path.GetDirectoryName(executablePath);

    if (directory is null)
    {
      return false;
    }

    var executableName = Path.GetFileNameWithoutExtension(executablePath);
    var companionDllPath = Path.Combine(directory, $"{executableName}.dll");

    if (File.Exists(companionDllPath))
    {
      return true;
    }

    var runtimeConfigPath = Path.Combine(directory, $"{executableName}.runtimeconfig.json");
    var depsPath = Path.Combine(directory, $"{executableName}.deps.json");

    return !File.Exists(runtimeConfigPath) && !File.Exists(depsPath);
  }

  private static IEnumerable<ExecutableCandidate> EnumerateExecutableCandidates(string projectName)
  {
    var baseDirectory = AppContext.BaseDirectory;
    var executableNames = GetExecutableNames(projectName);
    var dllName = $"{projectName}.dll";

    foreach (var executableName in executableNames)
    {
      yield return new ExecutableCandidate(Path.Combine(baseDirectory, executableName));
      yield return new ExecutableCandidate(Path.Combine(baseDirectory, projectName, executableName));
    }

    yield return new ExecutableCandidate(Path.Combine(baseDirectory, dllName));
    yield return new ExecutableCandidate(Path.Combine(baseDirectory, projectName, dllName));

    var repoRoot = FindRepositoryRoot(baseDirectory);

    if (repoRoot is null)
    {
      yield break;
    }

    var configuration = IsDebugBuild() ? "Debug" : "Release";
    var targetFramework = "net10.0";
    var outputDirectory = Path.Combine(repoRoot, projectName, "bin", configuration, targetFramework);

    foreach (var executableName in executableNames)
    {
      yield return new ExecutableCandidate(Path.Combine(outputDirectory, executableName));
    }

    yield return new ExecutableCandidate(Path.Combine(outputDirectory, dllName));
  }

  private static string[] GetExecutableNames(string projectName)
  {
    return OperatingSystem.IsWindows()
      ? [$"{projectName}.exe", projectName]
      : [projectName, $"{projectName}.exe"];
  }

  private static string? FindRepositoryRoot(string startDirectory)
  {
    var directory = new DirectoryInfo(startDirectory);

    while (directory is not null)
    {
      if (File.Exists(Path.Combine(directory.FullName, "MyHeroesClicker.slnx")))
      {
        return directory.FullName;
      }

      directory = directory.Parent;
    }

    return null;
  }

  private static bool IsDebugBuild()
  {
#if DEBUG
    return true;
#else
    return false;
#endif
  }

  private static void OpenExternalUri(Uri uri)
  {
    if (OperatingSystem.IsWindows())
    {
      Process.Start(new ProcessStartInfo(uri.ToString())
      {
        UseShellExecute = true
      });

      return;
    }

    if (OperatingSystem.IsMacOS())
    {
      Process.Start("open", uri.ToString());
      return;
    }

    Process.Start("xdg-open", uri.ToString());
  }

  private bool ShouldOpenExternally(Uri targetUri)
  {
    if (!targetUri.IsAbsoluteUri || string.Equals(targetUri.Scheme, "about", StringComparison.OrdinalIgnoreCase))
    {
      return false;
    }

    return _applicationUri is null
      || !string.Equals(targetUri.Host, _applicationUri.Host, StringComparison.OrdinalIgnoreCase)
      || targetUri.Port != _applicationUri.Port;
  }

  private static void ShutdownApplication()
  {
    if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      desktop.Shutdown();
    }
  }

  private sealed record ExecutableCandidate(string ExecutablePath);

  private sealed record ExecutableCommand(string FileName, string Arguments);

}
