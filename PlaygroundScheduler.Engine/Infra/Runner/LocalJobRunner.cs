using System.Diagnostics;
using PlaygroundScheduler.Engine.Domain.Identity;
using PlaygroundScheduler.Engine.Infra.Registry;
using PlaygroundScheduler.Engine.Infra.Repository;
using PlaygroundScheduler.Engine.Infra.Store;

namespace PlaygroundScheduler.Engine.Infra.Runner;

public class LocalJobRunner : ILocalJobRunner
{
    public List<JobRunId> StartedRunIds { get; } = new();
    public List<JobRunId> CancelledRunIds { get; } = new();

    private readonly IJobRunRepository _jobRunRepository;
    private readonly IJobDefinitionRepository _jobDefinitionRepository;
    private readonly IRunningJobRegistry _runningJobRegistry;
    private readonly IClock _clock;
    private readonly IJobRunOutputStore _jobRunOutputStore;
    

    public LocalJobRunner(IJobRunRepository jobRunRepository, IJobDefinitionRepository jobDefinitionRepository,
        IClock clock, IRunningJobRegistry runningJobRegistry, IJobRunOutputStore jobRunOutputStore)
    {
        _jobRunRepository = jobRunRepository;
        _jobDefinitionRepository = jobDefinitionRepository;
        _clock = clock;
        _runningJobRegistry = runningJobRegistry;
        _jobRunOutputStore= jobRunOutputStore;
        
    }

    public async Task StartAsync(JobRunId runId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var run = await _jobRunRepository.GetByIdAsync(runId, ct);
        ArgumentNullException.ThrowIfNull(run);
        var definition = await _jobDefinitionRepository.GetByIdAsync(run.JobDefinitionId, ct);

        if (definition is null)
            throw new InvalidOperationException($"Job definition '{run.JobDefinitionId}' was not found.");

        // Create process
        var psi = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-lc \"{definition.CommandLine}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = psi;
        process.EnableRaisingEvents = true;

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        if (!process.Start())
            throw new InvalidOperationException("Process failed to start.");

        var handle = new RunningJobHandle(process, linkedCts);

        if (!_runningJobRegistry.TryRegister(runId, handle))
            throw new InvalidOperationException($"Run '{runId}' is already registered as running.");
        try
        {
            run.MarkRunning(_clock.UtcNow);
            await _jobRunRepository.UpdateAsync(run, ct);
            StartedRunIds.Add(runId);

            await process.WaitForExitAsync(ct);
            var stdErr = await process.StandardError.ReadToEndAsync(ct);
            var refreshedRun = await _jobRunRepository.GetByIdAsync(runId, ct);
            if (refreshedRun is null)
                throw new InvalidOperationException($"Run '{runId}' was not found after process exit.");

            if (refreshedRun.RunStatus == RunStatus.Cancelled)
                return;

            if (process.ExitCode == 0)
            {
                refreshedRun.MarkSucceeded(_clock.UtcNow, process.ExitCode);
            }
            else
            {
                var error = !string.IsNullOrEmpty(stdErr) ? stdErr : $"Process exited with code {process.ExitCode}.";
                refreshedRun.MarkFailed(_clock.UtcNow, error, process.ExitCode);
            }

            await _jobRunRepository.UpdateAsync(refreshedRun, ct);
        }
        finally
        {
            _runningJobRegistry.TryRemove(runId, out _);
        }
    }

    public async Task CancelAsync(JobRunId runId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var run = await _jobRunRepository.GetByIdAsync(runId, ct);
        if (run is null)
            throw new InvalidOperationException($"Run '{runId}' was not found.");

        if (!_runningJobRegistry.TryGet(runId, out var handle) || handle is null)
            throw new InvalidOperationException($"Run '{runId}' is not currently running.");
        
        run.MarkCancelled(_clock.UtcNow, "Cancelled");
        CancelledRunIds.Add(runId);
        await _jobRunRepository.UpdateAsync(run, ct);
        
        try
        {
            if (!handle.Process.HasExited)
            {
                handle.Process.Kill(entireProcessTree: true);
                await handle.Process.WaitForExitAsync(CancellationToken.None);
            }
        }
        finally
        {
            _runningJobRegistry.TryRemove(runId, out _);
        }
    }
}