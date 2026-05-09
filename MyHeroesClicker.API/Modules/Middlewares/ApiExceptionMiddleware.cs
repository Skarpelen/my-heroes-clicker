using System.Net;
using System.Text.Json;
using NLog;

namespace MyHeroesClicker.API.Modules.Middlewares;

public sealed class ApiExceptionMiddleware
{
  private readonly RequestDelegate _next;
  private readonly Logger _log = LogManager.GetCurrentClassLogger();

  public ApiExceptionMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception exception)
    {
      _log.Error(exception, "Unhandled API exception.");

      if (context.Response.HasStarted)
      {
        throw;
      }

      context.Response.Clear();
      context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
      context.Response.ContentType = "application/json";

      var response = JsonSerializer.Serialize(new
      {
        error = exception.Message
      });

      await context.Response.WriteAsync(response);
    }
  }
}
