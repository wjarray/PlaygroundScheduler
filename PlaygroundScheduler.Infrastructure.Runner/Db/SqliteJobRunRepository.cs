using PlaygroundScheduler.Application.Repository;
using PlaygroundScheduler.Domain.Identity;
using PlaygroundScheduler.Infrastructure.Runner.Db.Connection;

namespace PlaygroundScheduler.Infrastructure.Runner.Db;

public class SqliteJobRunRepository : IJobRunRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public SqliteJobRunRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<JobRun?> GetByIdAsync(JobRunId id, CancellationToken ct = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        var command = connection.CreateCommand();

        command.CommandText = """
                              SELECT id,job_definition_id, created_at
                              FROM job_run
                              WHERE id = $id
                              """;

        command.Parameters.AddWithValue("$id", id.Value);
        await using var reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
            return null;

        return new JobRun(
                new JobRunId(Guid.Parse(reader.GetString(0))),
                new JobDefinitionId(Guid.Parse(reader.GetString(1))),
                DateTimeOffset.Parse(reader.GetString(2)));
        
    }

    public Task<IReadOnlyList<JobRun>> ListByJobIdAsync(JobDefinitionId id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<JobRun>> ListRecentAsync(int count, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task CreateAsync(JobRun run, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(JobRun run, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}