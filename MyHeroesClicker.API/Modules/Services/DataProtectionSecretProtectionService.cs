using Microsoft.AspNetCore.DataProtection;
using MyHeroesClicker.Core.Interfaces.Services;

namespace MyHeroesClicker.API.Modules.Services;

public sealed class DataProtectionSecretProtectionService : ISecretProtectionService
{
  private const string Prefix = "dataprotection:v1:";
  private readonly IDataProtector _protector;

  public DataProtectionSecretProtectionService(IDataProtectionProvider dataProtectionProvider)
  {
    _protector = dataProtectionProvider.CreateProtector("MyHeroesClicker.AccountPassword.v1");
  }

  public string Protect(string secret)
  {
    return Prefix + _protector.Protect(secret);
  }

  public string Unprotect(string protectedSecret)
  {
    if (!IsProtected(protectedSecret))
    {
      throw new InvalidOperationException("Пароль аккаунта сохранен в неподдерживаемом формате. Пересоздайте БД или сохраните пароль заново.");
    }

    return _protector.Unprotect(protectedSecret[Prefix.Length..]);
  }

  public bool IsProtected(string secret)
  {
    return secret.StartsWith(Prefix, StringComparison.Ordinal);
  }
}
