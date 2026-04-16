using PlaygroundScheduler.Engine.Domain.Identity;

namespace PlaygroundScheduler.Engine.Store;

public interface IJobRunOutputStore
{
    Task SaveAsync(JobRunOutput output, CancellationToken ct);
    Task<JobRunOutput?> GetByRunIdAsync(JobRunId runId, CancellationToken ct);
}