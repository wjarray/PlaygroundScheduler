using PlaygroundScheduler.Engine.Domain.Identity;

namespace PlaygroundScheduler.Engine.Runner;

public interface ILocalJobRunner
{
    Task StartAsync(JobRunId runId, CancellationToken ct = default);
    Task CancelAsync(JobRunId runId, CancellationToken ct = default);
}