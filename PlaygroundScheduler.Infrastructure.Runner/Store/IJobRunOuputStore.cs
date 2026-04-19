using PlaygroundScheduler.Domain.Identity;

namespace PlaygroundScheduler.Infrastructure.Runner.Store;

public interface IJobRunOutputStore
{
    Task SaveAsync(JobRunOutput output, CancellationToken? ct);
    Task<JobRunOutput?> GetByRunIdAsync(JobRunId runId, CancellationToken? ct);
}