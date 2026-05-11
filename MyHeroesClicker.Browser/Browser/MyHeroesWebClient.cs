using Microsoft.Playwright;
using MyHeroesClicker.Core.Confs;
using MyHeroesClicker.Core.Models.Browser;

namespace MyHeroesClicker.Browser.Browser;

public sealed class MyHeroesWebClient
{
  private readonly BrowserSession _browserSession;
  private readonly HttpClient _httpClient;
  private readonly Uri _baseUri;

  public MyHeroesWebClient(BrowserSession browserSession, ClickerOptions options)
  {
    _browserSession = browserSession;
    _baseUri = new Uri(options.BaseUrl);
    _httpClient = new HttpClient(new HttpClientHandler
    {
      AllowAutoRedirect = false,
      UseCookies = false
    })
    {
      BaseAddress = _baseUri
    };
  }

  public async Task<string> GetDocumentAsync(string path, CancellationToken cancellationToken)
  {
    using var response = await GetAsync(path, cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
      throw new InvalidOperationException($"Запрос {path} завершился с кодом {(int)response.StatusCode}.");
    }

    return await response.Content.ReadAsStringAsync(cancellationToken);
  }

  public async Task GetExpectedAsync(string path, CancellationToken cancellationToken)
  {
    var result = await TryGetExpectedAsync(path, cancellationToken);

    if (!result.IsExpected)
    {
      throw new InvalidOperationException($"Запрос {path} завершился с кодом {result.StatusCode}.");
    }
  }

  public async Task<CommandResponse> TryGetExpectedAsync(string path, CancellationToken cancellationToken)
  {
    using var response = await GetAsync(path, cancellationToken);

    return new CommandResponse((int)response.StatusCode, IsExpectedStatusCode(response));
  }

  public async Task PostFormExpectedAsync(
    string path,
    IReadOnlyDictionary<string, string> form,
    CancellationToken cancellationToken)
  {
    using var response = await SendAsync(HttpMethod.Post, path, new FormUrlEncodedContent(form), cancellationToken);

    if (!IsExpectedStatusCode(response))
    {
      throw new InvalidOperationException($"Запрос {path} завершился с кодом {(int)response.StatusCode}.");
    }
  }

  private async Task<HttpResponseMessage> GetAsync(string path, CancellationToken cancellationToken)
  {
    return await SendAsync(HttpMethod.Get, path, null, cancellationToken);
  }

  private async Task<HttpResponseMessage> SendAsync(
    HttpMethod method,
    string path,
    HttpContent? content,
    CancellationToken cancellationToken)
  {
    var requestUri = CreateUri(path);
    using var request = new HttpRequestMessage(method, requestUri)
    {
      Content = content
    };
    var cookieHeader = await BuildCookieHeaderAsync(cancellationToken);

    if (!string.IsNullOrWhiteSpace(cookieHeader))
    {
      request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
    }

    request.Headers.Referrer = _baseUri;

    var response = await _httpClient.SendAsync(request, cancellationToken);
    await ApplyResponseCookiesAsync(response, requestUri, cancellationToken);

    return response;
  }

  private Uri CreateUri(string path)
  {
    if (Uri.TryCreate(path, UriKind.Absolute, out var absoluteUri))
    {
      return absoluteUri;
    }

    return new Uri(_baseUri, path);
  }

  private async Task<string> BuildCookieHeaderAsync(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    var cookies = await _browserSession.Context.CookiesAsync([_baseUri.ToString()]);
    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    return string.Join(
      "; ",
      cookies
        .Where(cookie => cookie.Expires is <= 0 || cookie.Expires > now)
        .Select(cookie => $"{cookie.Name}={cookie.Value}"));
  }

  private async Task ApplyResponseCookiesAsync(
    HttpResponseMessage response,
    Uri requestUri,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
    {
      return;
    }

    var cookies = setCookieHeaders
      .Select(header => TryCreateCookie(header, requestUri))
      .Where(cookie => cookie is not null)
      .Select(cookie => cookie!)
      .ToArray();

    if (cookies.Length == 0)
    {
      return;
    }

    await _browserSession.Context.AddCookiesAsync(cookies);
  }

  private Cookie? TryCreateCookie(string setCookieHeader, Uri requestUri)
  {
    var parts = setCookieHeader.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    var nameValue = parts.FirstOrDefault()?.Split('=', 2);

    if (nameValue is not { Length: 2 } || string.IsNullOrWhiteSpace(nameValue[0]))
    {
      return null;
    }

    var path = parts
      .Select(part => part.Split('=', 2))
      .FirstOrDefault(part => part.Length == 2 && part[0].Equals("path", StringComparison.OrdinalIgnoreCase))?[1];

    return new Cookie
    {
      Name = nameValue[0],
      Value = nameValue[1],
      Url = new Uri(_baseUri, path ?? requestUri.AbsolutePath).ToString()
    };
  }

  private static bool IsExpectedStatusCode(HttpResponseMessage response)
  {
    return response.IsSuccessStatusCode
           || (int)response.StatusCode is >= 300 and < 400;
  }
}
