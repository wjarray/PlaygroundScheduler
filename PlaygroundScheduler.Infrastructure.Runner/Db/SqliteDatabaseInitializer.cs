using Microsoft.Data.Sqlite;
using PlaygroundScheduler.Infrastructure.Runner.Db.Connection;

namespace PlaygroundScheduler.Infrastructure.Runner.Db;

public sealed class SqliteDatabaseInitializer : IDatabaseInitializer
{
    private readonly IDatabasePathProvider _pathProvider;

    public SqliteDatabaseInitializer(IDatabasePathProvider pathProvider)
    {
        _pathProvider = pathProvider;
    }

    public async Task EnsureCreatedAsync(CancellationToken ct = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_pathProvider.DatabasePath)!);

        await using var connection = new SqliteConnection(
            $"Data Source={_pathProvider.DatabasePath}");

        await connection.OpenAsync(ct);

        var command = connection.CreateCommand();
        command.CommandText = """
                                  CREATE TABLE IF NOT EXISTS job_definition (
                                      id TEXT PRIMARY KEY,
                                      name TEXT NOT NULL,
                                      command_line TEXT NOT NULL
                                  );

                                  CREATE TABLE IF NOT EXISTS job_run (
                                      id TEXT PRIMARY KEY,
                                      job_definition_id TEXT NOT NULL,
                                      status TEXT NOT NULL,
                                      created_at TEXT NOT NULL,
                                      started_at TEXT NULL,
                                      completed_at TEXT NULL,
                                      FOREIGN KEY(job_definition_id) REFERENCES job_definition(id)
                                  );
                              """;

        await command.ExecuteNonQueryAsync(ct);
    }
}