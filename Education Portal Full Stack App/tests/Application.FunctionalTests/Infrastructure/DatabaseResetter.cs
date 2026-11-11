using EducationPortal.Infrastructure.Data.Lookups;
using Npgsql;
using Respawn;
using System.Data.Common;

namespace EducationPortal.Application.FunctionalTests.Infrastructure;

internal sealed class DatabaseResetter : IAsyncDisposable
{
    private readonly DbConnection _connection;
    private readonly Respawner _respawner;

    private DatabaseResetter(DbConnection connection, Respawner respawner)
    {
        _connection = connection;
        _respawner = respawner;
    }

    public static async Task<DatabaseResetter> CreateAsync(string connectionString)
    {
        var connection = new NpgsqlConnection(connectionString);

        await connection.OpenAsync();

        // The security audit log is append-only (SEC-LOG-05), so the reset leaves it alone. Each test reads only the
        // records of the users it created. The enum lookup tables are reference data seeded with the schema (ENG-ENUM-04).
        var respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            TablesToIgnore = [new Respawn.Graph.Table("SecurityAuditEvents"), .. EnumLookupTables.Tables.Select(name => new Respawn.Graph.Table(name))],
        });

        await connection.CloseAsync();
        return new DatabaseResetter(connection, respawner);
    }

    public async Task ResetAsync()
    {
        await _connection.OpenAsync();
        await _respawner.ResetAsync(_connection);
        await _connection.CloseAsync();
    }

    public async ValueTask DisposeAsync() => await _connection.DisposeAsync();
}
