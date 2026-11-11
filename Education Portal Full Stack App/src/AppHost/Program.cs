using EducationPortal.Shared;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment("aca-env");

var databaseServer = builder
    .AddAzurePostgresFlexibleServer(Services.DatabaseServer)
    .WithPasswordAuthentication()
    .RunAsContainer(container => 
        container.WithLifetime(ContainerLifetime.Persistent))
    .AddDatabase(Services.Database);

var web = builder.AddProject<Projects.Web>(Services.WebApi)
    .WithReference(databaseServer)
    .WaitFor(databaseServer)
    .WithExternalHttpEndpoints()
    .WithAspNetCoreEnvironment()
    .WithUrlForEndpoint("http", url =>
    {
        url.DisplayText = "Scalar API Reference";
        url.Url = "/scalar";
    });

if (builder.ExecutionContext.IsRunMode)
{
    // Local secret store: HashiCorp Vault in dev mode, which serves KV version 2 at "secret/" (ADR-006). Dev mode keeps
    // data in memory, so run scripts/vault-seed after a new container starts. The root token is generated once and
    // kept in the AppHost's user-secrets, never in the repository (SEC-SECRET-01, SEC-SECRET-04). Deployed
    // environments use the cloud store set by Secrets:Provider instead.
    var vaultToken = builder.AddParameter(
        Services.VaultDevToken,
        new GenerateParameterDefault { MinLength = 32, Special = false },
        secret: true,
        persist: true);

    var vault = builder.AddContainer(Services.Vault, "hashicorp/vault", "2.2.1")
        .WithLifetime(ContainerLifetime.Persistent)
        .WithHttpEndpoint(port: 8200, targetPort: 8200, name: "http")
        .WithEnvironment("VAULT_DEV_ROOT_TOKEN_ID", vaultToken)
        .WithEnvironment("VAULT_DEV_LISTEN_ADDRESS", "0.0.0.0:8200")
        .WithHttpHealthCheck("/v1/sys/health");

    web.WithEnvironment("Secrets__Provider", "HashiCorp")
        .WithEnvironment("Secrets__HashiCorp__Address", vault.GetEndpoint("http"))
        .WithEnvironment("Secrets__HashiCorp__AuthMethod", "Token")
        .WithEnvironment("Secrets__HashiCorp__Token", vaultToken)
        .WaitFor(vault);

    builder.AddJavaScriptApp(Services.WebFrontend, "./../Web/ClientApp")
        .WithRunScript("start")
        .WithReference(web)
        .WaitFor(web)
        .WithHttpEndpoint(env: "PORT")
        .WithExternalHttpEndpoints();
}

builder.Build().Run();
