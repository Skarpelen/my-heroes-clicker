namespace MyHeroesClicker.Core.Interfaces.Services;

public interface ISecretProtectionService
{
  string Protect(string secret);

  string Unprotect(string protectedSecret);

  bool IsProtected(string secret);
}
