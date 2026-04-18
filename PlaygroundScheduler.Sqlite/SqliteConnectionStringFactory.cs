namespace PlaygroundScheduler.Sqlite;

public class SqliteConnectionStringFactory(string connectionString)
{
    private readonly string _connectionString = connectionString;

    public string Create() => new(_connectionString);
}