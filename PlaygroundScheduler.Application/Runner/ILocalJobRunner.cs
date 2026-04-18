using PlaygroundScheduler.Domain.Identity;

namespace PlaygroundScheduler.Application.Runner;

public interface ILocalJobRunner
{
    Task StartAsync(JobRunId runId, CancellationToken ct = default);
    Task CancelAsync(JobRunId runId, CancellationToken ct = default);
}