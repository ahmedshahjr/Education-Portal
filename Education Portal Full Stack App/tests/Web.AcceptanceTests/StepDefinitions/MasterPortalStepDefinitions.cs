namespace EducationPortal.Web.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class MasterPortalStepDefinitions(MasterPortalPage masterPortalPage, LoginPage loginPage)
{
    [BeforeFeature("MasterPortal")]
    public static async Task BeforeMasterPortalFeature(IObjectContainer container)
    {
        var context = await PlaywrightSetup.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        container.RegisterInstanceAs(context);
        container.RegisterInstanceAs(new MasterPortalPage(page));
        container.RegisterInstanceAs(new LoginPage(page));
    }

    [AfterFeature("MasterPortal")]
    public static async Task AfterMasterPortalFeature(IObjectContainer container)
    {
        var context = container.Resolve<IBrowserContext>();
        await context.DisposeAsync();
    }

    [Given("a signed-in master administrator")]
    public async Task GivenASignedInMasterAdministrator()
    {
        await loginPage.GotoAsync();
        await loginPage.SignInAsDevelopmentAdministrator();
    }

    [When("the user opens the {string} page")]
    public Task WhenTheUserOpensThePage(string path) => masterPortalPage.OpenAsync(path);

    [Then("the master page shows the heading {string}")]
    public Task ThenTheMasterPageShowsTheHeading(string heading) => masterPortalPage.AssertHeadingVisible(heading);

    [Then("the sign-in form is shown")]
    public Task ThenTheSignInFormIsShown() => masterPortalPage.AssertSignInFormVisible();
}
