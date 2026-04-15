using PlaygroundScheduler.Engine.Domain.Identity;
using PlaygroundScheduler.Engine.Repository;
using PlaygroundScheduler.Engine.Runner;

namespace PlaygroundScheduler.Engine.Services;

public class JobRunService : IJobRunService
{
    private readonly IJobDefinitionRepository _jobDefinitionRepository;
    private readonly IJobRunRepository _jobRunRepository;
    private readonly IClock _clock;
    private readonly ILocalJobRunner _localJobRunner;

    public JobRunService(
        IJobDefinitionRepository jobDefinitionRepository,
        IJobRunRepository jobRunRepository,
        ILocalJobRunner localJobRunner,
        IClock clock)

    {
        _jobDefinitionRepository = jobDefinitionRepository;
        _jobRunRepository = jobRunRepository;
        _localJobRunner = localJobRunner;
        _clock = clock;
    }


    public async Task<JobRunId> CreateRunAsync(JobDefinitionId jobDefinitionId, CancellationToken ct = default)
    {
        var definition = await _jobDefinitionRepository.GetByIdAsync(jobDefinitionId, ct);
        if (definition is null)
            throw new InvalidOperationException($"Job definition '{jobDefinitionId}' was not found.");

        var run = new JobRun(JobRunId.New(), jobDefinitionId, _clock.UtcNow);

        await _jobRunRepository.CreateAsync(run, ct);
        return run.Id;
    }

    public async Task StartRunAsync(JobRunId runId, CancellationToken ct = default)
    {
        var run = await _jobRunRepository.GetByIdAsync(runId, ct);
        if (run is null)
            throw new InvalidOperationException($"Job run '{runId}' was not found.");

        if (run.RunStatus != RunStatus.Pending)
            throw new InvalidOperationException(
                $"Only pending runs can be started. Current status: {run.RunStatus}");

        await _localJobRunner.StartAsync(runId, ct);
    }

    public async Task CancelRunAsync(JobRunId runId, CancellationToken ct = default)
    {
        var run = await _jobRunRepository.GetByIdAsync(runId, ct);
        if (run is null)
            throw new InvalidOperationException($"Job run '{runId}' was not found.");

        if (run.RunStatus == RunStatus.Pending)
        {
            run.MarkCancelled(_clock.UtcNow, "Cancelled before start");
            await _jobRunRepository.UpdateAsync(run, ct);
            return;
        }

        if (run.RunStatus == RunStatus.Running)
        {
            await _localJobRunner.CancelAsync(runId, ct);
            return;
        }

        throw new InvalidOperationException(
            $"Only pending or running runs can be cancelled. Current status: {run.RunStatus}");
    }

    public Task<JobRun?> GetRunAsync(JobRunId runId, CancellationToken ct = default)  => _jobRunRepository.GetByIdAsync(runId, ct);

    public Task<IReadOnlyList<JobRun>> ListRunsAsync(JobDefinitionId jobDefinitionId, CancellationToken ct = default) =>
        _jobRunRepository.ListByJobIdAsync(jobDefinitionId, ct);
}

