using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MyHeroesClicker.API.Modules.Services;
using MyHeroesClicker.Core.Models.Alert;

namespace MyHeroesClicker.API.Modules.Controllers;

[ApiController]
[Route("api/alert")]
public sealed class AlertController : ControllerBase
{
  private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
  private readonly WebAlertService _alertService;

  public AlertController(WebAlertService alertService)
  {
    _alertService = alertService;
  }

  [HttpGet("stream")]
  public async Task StreamAsync(CancellationToken cancellationToken)
  {
    Response.ContentType = "text/event-stream";
    Response.Headers.CacheControl = "no-cache";

    var reader = _alertService.Subscribe(cancellationToken);

    try
    {
      await foreach (var alertEvent in reader.ReadAllAsync(cancellationToken))
      {
        var response = new AlertEventResponse
        {
          Id = alertEvent.Id,
          Kind = FormatKind(alertEvent.Kind),
          Message = alertEvent.Message,
          CreatedAt = alertEvent.CreatedAt
        };

        await Response.WriteAsync($"id: {alertEvent.Id}\n", cancellationToken);
        await Response.WriteAsync("event: alert\n", cancellationToken);
        await Response.WriteAsync($"data: {JsonSerializer.Serialize(response, JsonOptions)}\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
      }
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
    }
  }

  private static string FormatKind(AlertEventKind kind)
  {
    return kind switch
    {
      AlertEventKind.Captcha => "captcha",
      AlertEventKind.AuthenticationRequired => "authenticationRequired",
      AlertEventKind.FatalError => "fatalError",
      _ => kind.ToString()
    };
  }
}
