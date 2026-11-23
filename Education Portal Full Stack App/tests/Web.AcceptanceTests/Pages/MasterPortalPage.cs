namespace EducationPortal.Web.AcceptanceTests.Pages;

/// <summary>A master portal screen, opened by its path after sign-in on the master address.</summary>
public class MasterPortalPage(IPage page) : BasePage(page)
{
    public override string PagePath => BaseUrl;

    public Task OpenAsync(string path)
        => Page.GotoAsync($"{BaseUrl}{path}");

    public Task AssertHeadingVisible(string heading)
        => Assertions.Expect(Page.GetByRole(AriaRole.Heading, new() { Name = heading, Exact = true }).First).ToBeVisibleAsync();

    public Task AssertUrlPathIs(string path)
        => Assertions.Expect(Page).ToHaveURLAsync($"{BaseUrl}{path}");

    public Task AssertSignInFormVisible()
        => Assertions.Expect(Page.Locator("#email")).ToBeVisibleAsync();
}
