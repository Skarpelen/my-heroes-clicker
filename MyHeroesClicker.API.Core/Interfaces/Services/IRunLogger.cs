namespace MyHeroesClicker.Services;

public interface IRunLogger
{
  void Log(string message);

  void Warn(string message);

  void Error(string message);
}
