/// <summary>
/// Fixed host ports for every local resource, so each URL stays the same on every restart. Without a port Aspire picks a
/// random one, and a persistent container keeps that random port only until it is recreated.
/// Ports that are commonly taken by other tools on a developer machine (3000, 4200, 5432) get 10000 added to them.
/// The API keeps the ports in <c>src/Web/Properties/launchSettings.json</c> (https 7052, http 5132) and the dashboard
/// keeps the ports in <c>src/AppHost/Properties/launchSettings.json</c> (15251, 17080, 19025,
/// 20084, 21281, 22040). Every port in this file and those lists must be different; check before adding one.
/// </summary>
internal static class LocalPorts
{
    public const int DatabaseServer = 15432;
    public const int Vault = 8200;
    public const int WebFrontend = 14200;

    public const int Prometheus = 9090;
    public const int Loki = 3100;
    public const int JaegerUi = 16686;
    public const int JaegerOtlp = 14317;
    public const int CollectorGrpc = 4317;
    public const int CollectorHttp = 4318;
    public const int CollectorHealth = 13133;
    public const int Grafana = 13000;
}
