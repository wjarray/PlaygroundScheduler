
using PlaygroundScheduler.Engine.Domain.Identity;

namespace PlaygroundScheduler.Engine.Repository;

public interface IJobRunRepository
{
    Task<JobRun?> GetByIdAsync(JobRunId id, CancellationToken ct = default);
    Task<IReadOnlyList<JobRun>> ListByJobIdAsync(JobDefinitionId id, CancellationToken ct = default);
    
    Task<IReadOnlyList<JobRun>> ListRecentAsync(int count, CancellationToken ct = default);
    
    Task CreateAsync(JobRun run, CancellationToken ct = default);
    Task UpdateAsync(JobRun run, CancellationToken ct = default);
}