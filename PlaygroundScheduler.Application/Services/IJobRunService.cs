using PlaygroundScheduler.Domain.Identity;

namespace PlaygroundScheduler.Application.Services;

public interface IJobRunService
{
    Task<JobRunId> CreateRunAsync(JobDefinitionId jobDefinitionId, CancellationToken ct = default);
    Task StartRunAsync(JobRunId runId, CancellationToken ct = default);
    Task CancelRunAsync(JobRunId runId, CancellationToken ct = default);
    Task<JobRun?> GetRunAsync(JobRunId runId, CancellationToken ct = default);
    Task<IReadOnlyList<JobRun>> ListRunsAsync(JobDefinitionId jobDefinitionId, CancellationToken ct = default);
}

