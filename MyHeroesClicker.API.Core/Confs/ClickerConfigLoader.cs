using System.Text.Json;
using MyHeroesClicker.Services;

namespace MyHeroesClicker.Confs;

public static class ClickerConfigLoader
{
  private static readonly JsonSerializerOptions SerializerOptions = new()
  {
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true
  };

  public static ClickerConfig Load(ClickerOptions options, IRunLogger logger)
  {
    var configPath = ResolveConfigPath(options.ConfigPath);

    if (!File.Exists(configPath))
    {
      throw new InvalidOperationException(
        $"Не найден файл конфигов: {configPath}. Создайте его по примеру conf/clicker-config.example.json.");
    }

    logger.Log($"Загружаю конфиг: {configPath}");

    var json = File.ReadAllText(configPath);
    var config = JsonSerializer.Deserialize<ClickerConfig>(json, SerializerOptions)
                 ?? throw new InvalidOperationException($"Не удалось прочитать файл конфигов: {configPath}");

    Validate(config, configPath);

    return config;
  }

  private static string ResolveConfigPath(string configPath)
  {
    if (Path.IsPathFullyQualified(configPath))
    {
      return configPath;
    }

    var candidates = new[]
    {
      Path.GetFullPath(configPath),
      Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configPath))
    };

    return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
  }

  private static void Validate(ClickerConfig config, string configPath)
  {
    if (string.IsNullOrWhiteSpace(config.Login.UserName))
    {
      throw new InvalidOperationException($"В файле конфигов {configPath} не заполнено поле Login.UserName.");
    }

    if (string.IsNullOrWhiteSpace(config.Login.Password))
    {
      throw new InvalidOperationException($"В файле конфигов {configPath} не заполнено поле Login.Password.");
    }
  }
}
