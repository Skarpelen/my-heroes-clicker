using MyHeroesClicker.API.Services;
using MyHeroesClicker.Services;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Web;

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
