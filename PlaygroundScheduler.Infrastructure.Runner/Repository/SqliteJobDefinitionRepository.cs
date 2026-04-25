using Microsoft.Data.Sqlite;
using PlaygroundScheduler.Application.Repository;
using PlaygroundScheduler.Domain.Identity;
using PlaygroundScheduler.Infrastructure.Runner.Db.Connection;

namespace PlaygroundScheduler.Infrastructure.Runner.Repository;

public class SqliteJobDefinitionRepository : IJobDefinitionRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public SqliteJobDefinitionRepository(ISqliteConnectionFactory connectionFactory)
    {
        this._connectionFactory = connectionFactory;
    }
    
    public async Task<JobDefinition?> GetByIdAsync(JobDefinitionId id, CancellationToken ct = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        var command = connection.CreateCommand();
        command.CommandText = """
                                  SELECT id, name, command_line, retry_count
                                  FROM job_definition
                                  WHERE id = $id;
                              """;
        command.Parameters.AddWithValue("$id", id.Value.ToString());

        await using var reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
            return null;

        var jobGuid = new JobDefinitionId(Guid.Parse(reader.GetString(0)));
        var name = reader.GetString(1);
        var commandLine = reader.GetString(2);
        var maxRetryCount = reader.GetInt32(3);
        return new JobDefinition(jobGuid,name,commandLine, maxRetryCount);
    }

    public async Task<IReadOnlyList<JobDefinition>> ListAsync(CancellationToken ct = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        var command = connection.CreateCommand();
        command.CommandText = """
                                  SELECT id, name, command_line, retry_count
                                  FROM job_definition
                              """;

        await using var reader = await command.ExecuteReaderAsync(ct);

        List<JobDefinition> result = new List<JobDefinition>();
        if (!await reader.ReadAsync(ct))
            return result;

        while (reader.Read())
        {
            var jobGuid = new JobDefinitionId(Guid.Parse(reader.GetString(0)));
            var name = reader.GetString(1);
            var commandLine = reader.GetString(2);
            var maxRetryCount = reader.GetInt32(3);
            result.Add(new JobDefinition(jobGuid, name, commandLine, maxRetryCount));
        }

        return result;
    }

    public async Task UpdateAsync(JobDefinition job, CancellationToken ct = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        var command = connection.CreateCommand();
        
        command.CommandText = """
                              UPDATE job_definition
                              SET
                                  name = $jobName,
                                  command_line = $commandLine,
                                  retry_count = $maxRetryCount
                              WHERE id = $jobDefinitionId;
                              """;

        command.Parameters.Add(new SqliteParameter("$jobDefinitionId", job.DefinitionId.Value.ToString()));
        command.Parameters.Add(new SqliteParameter("$jobName", job.Name));
        command.Parameters.Add(new SqliteParameter("$commandLine", job.CommandLine));
        command.Parameters.Add(new SqliteParameter("$maxRetryCount", job.MaxRetryCount));

        var affectedRows = await command.ExecuteNonQueryAsync(ct);

        if (affectedRows != 1)
            throw new InvalidOperationException($"Job definition '{job.DefinitionId.Value}' was not found.");
    }

    public async Task CreateAsync(JobDefinition job, CancellationToken ct = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();

        command.CommandText = """
                              INSERT INTO job_definition (
                                  id,
                                  name,
                                  command_line,
                                  retry_count
                              )
                              VALUES (
                                         $jobDefinitionId,
                                         $jobName,
                                         $commandLine,
                                         $retryCount
                                     );
                              """;

        command.Parameters.Add(new SqliteParameter("$jobDefinitionId", job.DefinitionId.Value.ToString()));
        command.Parameters.Add(new SqliteParameter("$jobName", job.Name));
        command.Parameters.Add(new SqliteParameter("$commandLine", job.CommandLine));
        command.Parameters.Add(new SqliteParameter("$retryCount", job.MaxRetryCount));

        var affectedRows = await command.ExecuteNonQueryAsync(ct);

        if (affectedRows != 1)
            throw new InvalidOperationException($"Insert failed. affectedRows={affectedRows}");
    }
    
    public async Task DeleteAsync(JobDefinitionId definitionId, CancellationToken ct = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        var command = connection.CreateCommand();
        
        command.CommandText = """
                                 DELETE
                                 FROM job_definition
                                 WHERE id = $jobDefinitionId
                              """;
        
        command.Parameters.AddWithValue("$jobDefinitionId", definitionId.Value.ToString());

        await using var reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
            throw new Exception("Delete raté");
    }
}