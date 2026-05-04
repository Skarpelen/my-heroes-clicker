namespace MyHeroesClicker.Services;

public sealed class ConsoleRunLogger : IRunLogger
{
  public void Log(string message)
  {
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
  }
}