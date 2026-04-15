using PlaygroundScheduler.Engine.Domain.Identity;

namespace PlaygroundScheduler.Engine.Runner;

public sealed class FakeLocalJobRunner : ILocalJobRunner
{
    public List<JobRunId> StartedRunIds { get; } = new();
    public List<JobRunId> CancelledRunIds { get; } = new();

    public Task StartAsync(JobRunId runId, CancellationToken ct = default)
    {
        StartedRunIds.Add(runId);
        return Task.CompletedTask;
    }

    public Task CancelAsync(JobRunId runId, CancellationToken ct = default)
    {
        CancelledRunIds.Add(runId);
        return Task.CompletedTask;
    }
}