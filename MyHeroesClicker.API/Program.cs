using MyHeroesClicker.Core.Interfaces.Repositories;
using MyHeroesClicker.DataSQLite.Repositories;
using MyHeroesClicker.DataSQLite.Toolkit;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Web;
using MyHeroesClicker.Core.Interfaces.Services;
using MyHeroesClicker.API.Modules.Services;
using MyHeroesClicker.Core.Confs;

namespace MyHeroesClicker.API;

public class Program
{
  public static void Main(string[] args)
  {
    ConfigureNLog();

    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services.AddControllers();
    builder.Services.AddSingleton(serviceProvider =>
    {
      var options = new ClickerOptions();
      serviceProvider.GetRequiredService<IConfiguration>().GetSection("Clicker").Bind(options);

      return options;
    });

    builder.Services.AddSingleton<IRunLogger, ScenarioRunLogger>();
    builder.Services.AddSingleton<IAlertService, WebAlertService>();
    builder.Services.AddSingleton<IPauseService, WebPauseService>();
    builder.Services.AddSingleton<ClickerApiService>();
    builder.Services.AddSingleton(serviceProvider =>
    {
      var configuration = serviceProvider.GetRequiredService<IConfiguration>();
      var databasePath = configuration["Database:Path"] ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MyHeroesClicker",
        "clicker.sqlite");

      return new SqliteConnectionFactory(databasePath);
    });
    builder.Services.AddScoped<IAccountRepository, AccountRepository>();
    builder.Services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();
    builder.Services.AddScoped<IEquipmentSetRepository, EquipmentSetRepository>();
    builder.Services.AddScoped<ITechniquePresetRepository, TechniquePresetRepository>();

    var app = builder.Build();

    app.MapControllers();

    app.Run();
  }

  private static void ConfigureNLog()
  {
    var config = new LoggingConfiguration();
    var consoleTarget = new ColoredConsoleTarget("console")
    {
      Layout = "[${date:format=HH\\:mm\\:ss}] ${uppercase:${level}} ${message}",
      UseDefaultRowHighlightingRules = true
    };

    config.AddRule(
      NLog.LogLevel.Info,
      NLog.LogLevel.Fatal,
      consoleTarget,
      typeof(ScenarioRunLogger).FullName!);

    LogManager.Configuration = config;
  }
}
