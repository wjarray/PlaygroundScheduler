using PlaygroundScheduler.Domain.Identity;

namespace PlaygroundScheduler.Engine.Infra.Store;

public class InMemoryJobRunOutputStore : IJobRunOutputStore
{
    private Dictionary<JobRunId, JobRunOutput> _jobRunOutputs = new();
    
    public Task SaveAsync(JobRunOutput output, CancellationToken? ct)
    {
        if (!_jobRunOutputs.TryAdd(output.RunId, output))
            throw new InvalidOperationException($"Output for run '{output.RunId}' already exists.");

        return Task.CompletedTask;
    }

    public Task<JobRunOutput?> GetByRunIdAsync(JobRunId runId, CancellationToken? ct)
    {
        _jobRunOutputs.TryGetValue(runId, out var value);
        return Task.FromResult(value);
    }
}