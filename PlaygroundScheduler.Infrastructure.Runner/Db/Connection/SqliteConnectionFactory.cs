using Microsoft.Data.Sqlite;

namespace PlaygroundScheduler.Infrastructure.Runner.Db.Connection;

public class SqliteConnectionFactory : ISqliteConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(IDatabasePathProvider pathProvider)
    {
        _connectionString = $"Data Source ={pathProvider.DatabasePath}";
    }
    
    public SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}