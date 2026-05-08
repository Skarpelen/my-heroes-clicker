using System.Text.RegularExpressions;

namespace MyHeroesClicker.Browser;

public sealed class CharacterStatsReader
{
  private static readonly Regex HealthRegex = new(
    @"здоровье:\s*(?<current>\d+)\s*/\s*(?<max>\d+)",
    RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

  private readonly MyHeroesWebClient _webClient;

  public CharacterStatsReader(MyHeroesWebClient webClient)
  {
    _webClient = webClient;
  }

  public async Task<int> ReadMaxHealthAsync(CancellationToken cancellationToken)
  {
    var html = await _webClient.GetDocumentAsync("/user/params", cancellationToken);
    var match = HealthRegex.Match(html);

    if (!match.Success || !int.TryParse(match.Groups["max"].Value, out var maxHealth))
    {
      throw new InvalidOperationException("Не удалось прочитать максимальное здоровье со страницы параметров.");
    }

    return maxHealth;
  }
}
