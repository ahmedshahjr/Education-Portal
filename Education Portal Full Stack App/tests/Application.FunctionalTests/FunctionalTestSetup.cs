using Microsoft.Extensions.DependencyInjection;

namespace EducationPortal.Application.FunctionalTests;

[SetUpFixture]
public class FunctionalTestSetup
{
    internal static IServiceScopeFactory ScopeFactory { get; private set; } = null!;
    internal static DatabaseResetter? DbResetter { get; private set; }
    internal static string ConnectionString { get; private set; } = string.Empty;
    internal static WebApiFactory DevelopmentFactory { get; private set; } = null!;
    internal static ProductionWebApiFactory ProductionFactory { get; private set; } = null!;

    private static DistributedApplication? _app;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        var cancellationToken = cts.Token;

        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.TestAppHost>(
                args: [],
                configureBuilder: (options, _) =>
                {
                    options.DisableDashboard = true;
                });

        builder.Configuration["ASPIRE_ALLOW_UNSECURED_TRANSPORT"] = "true";

        _app = await builder
            .BuildAsync(cancellationToken)
            .WaitAsync(cancellationToken);

        await _app
            .StartAsync(cancellationToken)
            .WaitAsync(cancellationToken);

        await _app.ResourceNotifications.WaitForResourceHealthyAsync(
            Services.Database, cancellationToken);

        ConnectionString = (await _app.GetConnectionStringAsync(Services.Database))!;

        // The Development host creates and seeds the schema first. The Production-mode host runs against that schema.
        DevelopmentFactory = new WebApiFactory(ConnectionString);
        ScopeFactory = DevelopmentFactory.Services.GetRequiredService<IServiceScopeFactory>();
        DbResetter = await DatabaseResetter.CreateAsync(ConnectionString);

        ProductionFactory = new ProductionWebApiFactory(ConnectionString);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (DbResetter is not null) await DbResetter.DisposeAsync();
        if (_app is not null) await _app.DisposeAsync();
        if (ProductionFactory is not null) await ProductionFactory.DisposeAsync();
        if (DevelopmentFactory is not null) await DevelopmentFactory.DisposeAsync();
    }
}
