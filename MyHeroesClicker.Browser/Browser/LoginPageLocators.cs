using Microsoft.Playwright;

namespace MyHeroesClicker.Browser.Browser;

public static class LoginPageLocators
{
  public static ILocator LoginForm(IPage page)
  {
    return page.Locator("form[action='/main/login'][method='post']");
  }

  public static ILocator LoginInput(IPage page)
  {
    return LoginForm(page).Locator("input[name='login']");
  }

  public static ILocator PasswordInput(IPage page)
  {
    return LoginForm(page).Locator("input[name='password']");
  }

  public static ILocator SubmitButton(IPage page)
  {
    return LoginForm(page).Locator("input[type='submit'][name='btn_login']");
  }
}
