namespace EducationPortal.Web.AcceptanceTests.Pages;

public class LoginPage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/login";

    public Task SetEmail(string email)
        => Page.FillAsync("#email", email);

    public Task SetPassword(string password)
        => Page.FillAsync("#password", password);

    public Task ClickLogin()
        => Page.Locator("button[type='submit']").ClickAsync();

    // Sign out lives in the account menu of the top bar, so open the menu first.
    public async Task<string?> SignOutButtonText()
    {
        await Page.Locator("button[aria-label='Account menu']").ClickAsync();
        var signOut = Page.GetByRole(AriaRole.Menuitem, new() { Name = "Sign out" });
        return (await signOut.TextContentAsync())?.Trim();
    }

    public Task AssertErrorVisible()
        => Assertions.Expect(Page.Locator("#login-error")).ToBeVisibleAsync();
}
