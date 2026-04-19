using Microsoft.Data.Sqlite;

namespace PlaygroundScheduler.Infrastructure.Runner.Db.Connection;

public interface ISqliteConnectionFactory
{
    SqliteConnection CreateConnection();
}