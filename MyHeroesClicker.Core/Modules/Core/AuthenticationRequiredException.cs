namespace MyHeroesClicker.Core.Modules.Core;

public sealed class AuthenticationRequiredException : Exception
{
  public AuthenticationRequiredException(string message)
    : base(message)
  {
  }
}
