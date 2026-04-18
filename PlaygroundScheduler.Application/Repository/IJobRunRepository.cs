using PlaygroundScheduler.Domain.Identity;

namespace PlaygroundScheduler.Application.Repository;

public interface IJobRunRepository
{
    Task<JobRun?> GetByIdAsync(JobRunId id, CancellationToken ct = default);
    Task<IReadOnlyList<JobRun>> ListByJobIdAsync(JobDefinitionId id, CancellationToken ct = default);
    
    Task<IReadOnlyList<JobRun>> ListRecentAsync(int count, CancellationToken ct = default);
    
    Task CreateAsync(JobRun run, CancellationToken ct = default);
    Task UpdateAsync(JobRun run, CancellationToken ct = default);
}