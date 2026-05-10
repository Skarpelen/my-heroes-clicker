using MyHeroesClicker.Core.Interfaces.Repositories;
using MyHeroesClicker.DataSQLite.Repositories;
using MyHeroesClicker.DataSQLite.Toolkit;
using NLog.Web;
using MyHeroesClicker.Core.Interfaces.Services;
using MyHeroesClicker.API.Modules.Middlewares;
using MyHeroesClicker.API.Modules.Services;
using MyHeroesClicker.Core.Confs;
using Microsoft.AspNetCore.DataProtection;

namespace MyHeroesClicker.API;

public class Program
{
  public static void Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services.AddControllers(options =>
    {
      options.SuppressAsyncSuffixInActionNames = false;
    });
    builder.Services.AddSingleton(serviceProvider =>
    {
      var options = new ClickerOptions();
      serviceProvider.GetRequiredService<IConfiguration>().GetSection("Clicker").Bind(options);

      return options;
    });

    builder.Services.AddSingleton<IRunLogger, ScenarioRunLogger>();
    builder.Services.AddSingleton<WebAlertService>();
    builder.Services.AddSingleton<IAlertService>(serviceProvider => serviceProvider.GetRequiredService<WebAlertService>());
    builder.Services.AddSingleton<IPauseService, WebPauseService>();
    builder.Services
      .AddDataProtection()
      .PersistKeysToFileSystem(new DirectoryInfo(GetDataProtectionKeysPath(builder.Configuration)))
      .SetApplicationName("MyHeroesClicker");
    builder.Services.AddSingleton<ISecretProtectionService, DataProtectionSecretProtectionService>();
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

    app.UseMiddleware<ApiExceptionMiddleware>();

    app.MapControllers();

    app.Run();
  }

  private static string GetDataProtectionKeysPath(IConfiguration configuration)
  {
    return configuration["DataProtection:KeysPath"] ?? Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
      "MyHeroesClicker",
      "keys");
  }
}
