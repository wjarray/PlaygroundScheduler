using Microsoft.Data.Sqlite;
using PlaygroundScheduler.Application.Repository;
using PlaygroundScheduler.Domain.Identity;
using PlaygroundScheduler.Infrastructure.Runner.Db.Connection;

namespace PlaygroundScheduler.Infrastructure.Runner.Db;

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
                                  SELECT id, name, command_line, max_retry_count
                                  FROM job_definition
                                  WHERE id = $id
                              """;
        command.Parameters.AddWithValue("$id", id.Value);

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
                                  SELECT id, name, command_line, max_retry_count
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
                                  INSERT INTO job_definition (id, name, command_line, max_retry_count)
                                  VALUES ($jobDefinitionId,$jobName,$commandLine,$maxRetryCount)
                                  WHERE id = $jobId
                              """;

        List<SqliteParameter> parameters = new();
        parameters.Add(new SqliteParameter("$jobDefinitionId", job.DefinitionId.Value));
        parameters.Add(new SqliteParameter("$jobName", job.Name));
        parameters.Add(new SqliteParameter("$commandLine", job.CommandLine));
        parameters.Add(new SqliteParameter("$maxRetryCount", job.MaxRetryCount));
        
        command.Parameters.AddRange(parameters);

        await using var reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
            throw new Exception("insert raté");
             
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
        
        command.Parameters.AddWithValue("$jobDefinitionId", definitionId.Value);

        await using var reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
            throw new Exception("Delete raté");
    }
}