namespace EducationPortal.Shared;

public static class Services
{
    /// <summary>
    /// The name of the Web Frontend service.
    /// This service is responsible for hosting the frontend application.
    /// </summary>
    public const string WebFrontend = "webfrontend";

    /// <summary>
    /// The name of the Web API service.
    /// This service is responsible for hosting the Web API application.
    /// </summary>
    public const string WebApi = "webapi";

    /// <summary>
    /// The name of the Database Server service.
    /// This service is responsible for hosting the PostgreSQL database server.
    /// </summary>
    public const string DatabaseServer = "dbserver";

    /// <summary>
    /// The name of the Database.
    /// This is the name of the database that will be created and used by the application.
    /// </summary>
    public const string Database = "EducationPortalDb";

    /// <summary>
    /// The name of the local HashiCorp Vault container (development only).
    /// It is the secret store behind ISecretProvider when running locally (ADR-006).
    /// </summary>
    public const string Vault = "vault";

    /// <summary>The name of the Aspire parameter that holds the local Vault's root token (development only).</summary>
    public const string VaultDevToken = "vault-dev-token";
}
