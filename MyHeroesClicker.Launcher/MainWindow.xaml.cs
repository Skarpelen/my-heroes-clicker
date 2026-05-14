using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using MyHeroesClicker.Launcher.Updates;

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

  private readonly ReleaseUpdateService _releaseUpdateService;
  private Process? _backendProcess;
  private Uri? _applicationUri;
  private ReleaseUpdate? _availableUpdate;
  private bool _isStarting;
  private bool _isCheckingUpdates;
  private bool _isInstallingUpdate;
  private readonly string _logFilePath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "MyHeroesClicker",
    "logs",
    "launcher.log");

  public MainWindow()
  {
    InitializeComponent();

    _releaseUpdateService = new ReleaseUpdateService(_updateHttpClient);

    var version = ReleaseUpdateService.GetCurrentVersion();
    VersionTextBlock.Text = version.Major == 0
      ? $"Версия {version} beta"
      : $"Версия {version}";
    SetStatus("Лаунчер готов к запуску.");
    AppendDiagnostic($"Лог лаунчера: {_logFilePath}");

    Loaded += MainWindow_OnLoaded;
  }

  private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
  {
    await CheckUpdatesAsync(showUpToDateMessage: false);
  }

  private async void StartButton_OnClick(object sender, RoutedEventArgs e)
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
      OpenBrowserButton.IsEnabled = true;
      SetStatus($"Backend уже запущен: {_applicationUri}");
      await OpenInterfaceAsync();

      return;
    }

    _isStarting = true;
    StartButton.IsEnabled = false;
    OpenBrowserButton.IsEnabled = false;
    ErrorTextBlock.Text = string.Empty;

    try
    {
      var port = GetFreeTcpPort();
      _applicationUri = new Uri($"http://127.0.0.1:{port}");

      SetStatus("Применение миграций SQLite...");
      await RunMigratorAsync();

      SetStatus($"Запуск backend на порту {port}...");
      _backendProcess = StartBackendProcess(port);

      SetStatus("Ожидание готовности backend...");
      await WaitForBackendAsync(_applicationUri);

      OpenBrowserButton.IsEnabled = true;
      StartButton.Content = "Запущено";
      SetStatus($"Backend готов: {_applicationUri}");

      await OpenInterfaceAsync();
    }
    catch (Exception exception)
    {
      ErrorTextBlock.Text = exception.Message;
      DiagnosticsExpander.IsExpanded = true;
      SetStatus("Запуск остановлен из-за ошибки.");
      StopBackend();
    }
    finally
    {
      StartButton.IsEnabled = _backendProcess is null || _backendProcess.HasExited;
      _isStarting = false;
    }
  }

  private async Task RunMigratorAsync()
  {
    var executable = ResolveExecutable("MyHeroesClicker.DbMigrator");
    var arguments = AddDevelopmentMigrationsPath(executable.Arguments);
    AppendDiagnostic($"Запуск мигратора: {executable.FileName} {arguments}");

    var result = await RunHiddenProcessAsync(executable.FileName, arguments);

    AppendProcessOutput("DbMigrator", result);

    if (result.ExitCode == 0)
    {
      return;
    }

    var error = string.IsNullOrWhiteSpace(result.Error)
      ? result.Output.Trim()
      : result.Error.Trim();

    if (string.IsNullOrWhiteSpace(error))
    {
      error = $"Код завершения: {result.ExitCode}.";
    }

    throw new InvalidOperationException($"Миграции SQLite завершились с ошибкой. {error}");
  }

  private Process StartBackendProcess(int port)
  {
    var executable = ResolveExecutable("MyHeroesClicker.API");
    var startInfo = new ProcessStartInfo(executable.FileName, executable.Arguments)
    {
      UseShellExecute = false,
      CreateNoWindow = true,
      WindowStyle = ProcessWindowStyle.Hidden,
      RedirectStandardError = true,
      RedirectStandardOutput = true,
      StandardErrorEncoding = Encoding.UTF8,
      StandardOutputEncoding = Encoding.UTF8
    };

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

  private static async Task<ProcessExecutionResult> RunHiddenProcessAsync(string fileName, string arguments)
  {
    var startInfo = new ProcessStartInfo(fileName, arguments)
    {
      UseShellExecute = false,
      CreateNoWindow = true,
      WindowStyle = ProcessWindowStyle.Hidden,
      RedirectStandardError = true,
      RedirectStandardOutput = true,
      StandardErrorEncoding = Encoding.UTF8,
      StandardOutputEncoding = Encoding.UTF8
    };

    var process = Process.Start(startInfo);

    if (process is null)
    {
      throw new InvalidOperationException($"Не удалось запустить процесс {fileName}.");
    }

    var outputTask = process.StandardOutput.ReadToEndAsync();
    var errorTask = process.StandardError.ReadToEndAsync();

    await process.WaitForExitAsync();

    var output = await outputTask;
    var error = await errorTask;

    return new ProcessExecutionResult(process.ExitCode, output, error);
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

  private async Task OpenInterfaceAsync()
  {
    if (_applicationUri is null)
    {
      return;
    }

    try
    {
      await Browser.EnsureCoreWebView2Async();
      Browser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
      Browser.CoreWebView2.Settings.AreDevToolsEnabled = false;
      Browser.Source = _applicationUri;
    }
    catch (WebView2RuntimeNotFoundException)
    {
      ErrorTextBlock.Text = "WebView2 Runtime не установлен. Откройте интерфейс во внешнем браузере.";
    }
  }

  private void OpenBrowserButton_OnClick(object sender, RoutedEventArgs e)
  {
    if (_applicationUri is null)
    {
      return;
    }

    OpenExternalUri(_applicationUri);
  }

  private async void CheckUpdatesButton_OnClick(object sender, RoutedEventArgs e)
  {
    await CheckUpdatesAsync(showUpToDateMessage: true);
  }

  private async void InstallUpdateButton_OnClick(object sender, RoutedEventArgs e)
  {
    if (_availableUpdate is null || _isInstallingUpdate)
    {
      return;
    }

    _isInstallingUpdate = true;
    CheckUpdatesButton.IsEnabled = false;
    InstallUpdateButton.IsEnabled = false;
    UpdateStatusTextBlock.Text = $"Скачивание версии {_availableUpdate.LatestVersion}...";

    try
    {
      await _releaseUpdateService.ApplyAsync(_availableUpdate, CancellationToken.None);
      UpdateStatusTextBlock.Text = "Обновление скачано. Лаунчер перезапустится.";
      StopBackend();
      Application.Current.Shutdown();
    }
    catch (Exception exception)
    {
      ErrorTextBlock.Text = $"Не удалось установить обновление. {exception.Message}";
      AppendDiagnostic(ErrorTextBlock.Text);
      CheckUpdatesButton.IsEnabled = true;
      InstallUpdateButton.IsEnabled = true;
      _isInstallingUpdate = false;
    }
  }

  private void Browser_OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
  {
    if (!Uri.TryCreate(e.Uri, UriKind.Absolute, out var targetUri))
    {
      return;
    }

    if (_applicationUri is not null
        && string.Equals(targetUri.Host, _applicationUri.Host, StringComparison.OrdinalIgnoreCase)
        && targetUri.Port == _applicationUri.Port)
    {
      return;
    }

    e.Cancel = true;
    OpenExternalUri(targetUri);
  }

  private void MainWindow_OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
  {
    StopBackend();
    _httpClient.Dispose();
    _updateHttpClient.Dispose();
  }

  private async Task CheckUpdatesAsync(bool showUpToDateMessage)
  {
    if (_isCheckingUpdates)
    {
      return;
    }

    _isCheckingUpdates = true;
    CheckUpdatesButton.IsEnabled = false;
    UpdateStatusTextBlock.Text = "Проверяю обновления...";

    try
    {
      var update = await _releaseUpdateService.CheckAsync(CancellationToken.None);
      _availableUpdate = update is { IsUpdateAvailable: true } ? update : null;
      RenderUpdateState(update, showUpToDateMessage);
    }
    catch (Exception exception)
    {
      UpdateStatusTextBlock.Text = "Не удалось проверить обновления.";
      AppendDiagnostic($"Проверка обновлений завершилась с ошибкой: {exception.Message}");
    }
    finally
    {
      CheckUpdatesButton.IsEnabled = true;
      _isCheckingUpdates = false;
    }
  }

  private void RenderUpdateState(ReleaseUpdate? update, bool showUpToDateMessage)
  {
    InstallUpdateButton.Visibility = Visibility.Collapsed;
    InstallUpdateButton.IsEnabled = false;
    ChangelogExpander.Visibility = Visibility.Collapsed;
    ChangelogTextBox.Text = string.Empty;

    if (update is null)
    {
      UpdateStatusTextBlock.Text = showUpToDateMessage
        ? "Опубликованных релизов пока не найдено."
        : string.Empty;

      return;
    }

    if (!update.IsUpdateAvailable)
    {
      UpdateStatusTextBlock.Text = showUpToDateMessage
        ? $"Установлена актуальная версия {update.CurrentVersion}."
        : string.Empty;

      return;
    }

    var versionLineMessage = update.IsNewVersionLine
      ? " Доступна новая ветка версии, обновление рекомендуется."
      : " Доступно патч-обновление.";

    UpdateStatusTextBlock.Text = $"Доступна версия {update.LatestVersion}.{versionLineMessage}";
    InstallUpdateButton.Visibility = Visibility.Visible;
    InstallUpdateButton.IsEnabled = true;

    if (string.IsNullOrWhiteSpace(update.Changelog))
    {
      return;
    }

    ChangelogTextBox.Text = update.Changelog.Trim();
    ChangelogExpander.Visibility = Visibility.Visible;
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
    StatusTextBlock.Text = message;
    AppendDiagnostic(message);
  }

  private void AppendProcessOutput(string source, ProcessExecutionResult result)
  {
    if (!string.IsNullOrWhiteSpace(result.Output))
    {
      AppendDiagnostic($"{source} stdout:{Environment.NewLine}{result.Output.Trim()}");
    }

    if (!string.IsNullOrWhiteSpace(result.Error))
    {
      AppendDiagnostic($"{source} stderr:{Environment.NewLine}{result.Error.Trim()}");
    }
  }

  private void AppendDiagnosticFromProcess(string source, string? message)
  {
    if (string.IsNullOrWhiteSpace(message))
    {
      return;
    }

    Dispatcher.Invoke(() => AppendDiagnostic($"{source}: {message}"));
  }

  private void AppendDiagnostic(string message)
  {
    var line = $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss} {message}";
    DiagnosticsTextBox.AppendText(line + Environment.NewLine);
    DiagnosticsTextBox.ScrollToEnd();

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
      if (File.Exists(candidate.ExecutablePath))
      {
        if (candidate.ExecutablePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
          return new ExecutableCommand("dotnet", $"\"{candidate.ExecutablePath}\"");
        }

        var expectedDllPath = Path.ChangeExtension(candidate.ExecutablePath, ".dll");

        if (!File.Exists(expectedDllPath))
        {
          continue;
        }

        return new ExecutableCommand(candidate.ExecutablePath, string.Empty);
      }
    }

    throw new FileNotFoundException($"Не найден исполняемый файл проекта {projectName}.");
  }

  private static IEnumerable<ExecutableCandidate> EnumerateExecutableCandidates(string projectName)
  {
    var baseDirectory = AppContext.BaseDirectory;
    var executableName = $"{projectName}.exe";
    var dllName = $"{projectName}.dll";

    yield return new ExecutableCandidate(Path.Combine(baseDirectory, executableName));
    yield return new ExecutableCandidate(Path.Combine(baseDirectory, dllName));
    yield return new ExecutableCandidate(Path.Combine(baseDirectory, projectName, executableName));
    yield return new ExecutableCandidate(Path.Combine(baseDirectory, projectName, dllName));

    var repoRoot = FindRepositoryRoot(baseDirectory);

    if (repoRoot is null)
    {
      yield break;
    }

    var configuration = IsDebugBuild() ? "Debug" : "Release";
    var targetFramework = projectName == "MyHeroesClicker.API" || projectName == "MyHeroesClicker.DbMigrator"
      ? "net10.0"
      : "net10.0-windows";
    var outputDirectory = Path.Combine(repoRoot, projectName, "bin", configuration, targetFramework);

    yield return new ExecutableCandidate(Path.Combine(outputDirectory, executableName));
    yield return new ExecutableCandidate(Path.Combine(outputDirectory, dllName));
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

  private static string AddDevelopmentMigrationsPath(string arguments)
  {
    var repoRoot = FindRepositoryRoot(AppContext.BaseDirectory);

    if (repoRoot is null)
    {
      return arguments;
    }

    var migrationsPath = Path.Combine(repoRoot, "MyHeroesClicker.DbMigrator", "Migrations");

    if (!Directory.Exists(migrationsPath))
    {
      return arguments;
    }

    return string.IsNullOrWhiteSpace(arguments)
      ? $"--migrations \"{migrationsPath}\""
      : $"{arguments} --migrations \"{migrationsPath}\"";
  }

  private static void OpenExternalUri(Uri uri)
  {
    Process.Start(new ProcessStartInfo(uri.ToString())
    {
      UseShellExecute = true
    });
  }

  private sealed record ExecutableCandidate(string ExecutablePath);

  private sealed record ExecutableCommand(string FileName, string Arguments);

  private sealed record ProcessExecutionResult(int ExitCode, string Output, string Error);
}
