namespace MyHeroesClicker.Core.Models.Browser;

public sealed class CommandResponse
{
  public CommandResponse(int statusCode, bool isExpected)
  {
    StatusCode = statusCode;
    IsExpected = isExpected;
  }

  public int StatusCode { get; }

  public bool IsExpected { get; }
}
