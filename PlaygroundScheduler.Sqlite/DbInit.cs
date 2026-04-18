using Microsoft.Data.Sqlite;

namespace PlaygroundScheduler.Sqlite;

public static class DbInit
{
    public static async Task Init(SqliteConnection connection, CancellationToken ct = default)
    {
        var command = connection.CreateCommand();

        command.CommandText = """
        CREATE TABLE IF NOT EXISTS job_definition (
            id TEXT PRIMARY KEY,
            name TEXT NOT NULL,
            command_line TEXT NOT NULL,
            max_retry_count INTEGER NOT NULL
        );

        CREATE TABLE IF NOT EXISTS job_run (
            id TEXT PRIMARY KEY,
            job_definition_id TEXT NOT NULL,
            run_status TEXT NOT NULL,
            created_at TEXT NOT NULL,
            started_at TEXT NOT NULL,
            ended_at TEXT NOT NULL,
            exit_code int NOT NULL,
            error_message TEXT NOT NULL,
            FOREIGN_KEY(job_definition_id) REFERENCES job_definition(id)
            );
""";
        
        await command.ExecuteNonQueryAsync(ct);
    }
}