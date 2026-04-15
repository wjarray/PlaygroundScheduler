using System.Diagnostics;
using PlaygroundScheduler.Engine.Domain.Identity;
using PlaygroundScheduler.Engine.Registry;
using PlaygroundScheduler.Engine.Repository;

namespace PlaygroundScheduler.Engine.Runner;

public class LocalJobRunner : ILocalJobRunner
{
    
    public List<JobRunId> StartedRunIds { get; } = new();
    public List<JobRunId> CancelledRunIds { get; } = new();
    
    private readonly IJobRunRepository _jobRunRepository;
    private readonly IJobDefinitionRepository _jobDefinitionRepository;
    private readonly IRunningJobRegistry _runningJobRegistry;
    private readonly IClock _clock;

    public LocalJobRunner(IJobRunRepository jobRunRepository,IJobDefinitionRepository jobDefinitionRepository, IClock clock, IRunningJobRegistry runningJobRegistry)
    {
        _jobRunRepository = jobRunRepository;
        _jobDefinitionRepository = jobDefinitionRepository;
        _clock = clock;
        _runningJobRegistry = runningJobRegistry;
    }
    
    public async Task StartAsync(JobRunId runId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var run = await _jobRunRepository.GetByIdAsync(runId, ct);
        ArgumentNullException.ThrowIfNull(run);
        var definition = await _jobDefinitionRepository.GetByIdAsync(run.JobDefinitionId,ct);
        
        if (definition is null)
            throw new InvalidOperationException($"Job definition '{run.JobDefinitionId}' was not found.");
        // Start le vrai travail
        var psi = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-lc \"{definition.CommandLine}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        var process = new Process
        {
            StartInfo = psi,
            EnableRaisingEvents = true
        };
        
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        try
        {
            if (!process.Start())
                throw new InvalidOperationException("Process failed to start.");
            
            var handle = new RunningJobHandle(process, linkedCts);
            
            if (!_runningJobRegistry.TryRegister(runId, handle))
                throw new InvalidOperationException($"Run '{runId}' is already registered as running.");
            
            run.MarkRunning(_clock.UtcNow);
            await _jobRunRepository.UpdateAsync(run,ct);
            
            StartedRunIds.Add(runId);
            await process.WaitForExitAsync(ct);
            if (process.ExitCode == 0)
                run.MarkSucceeded(_clock.UtcNow, process.ExitCode);
            else
                run.MarkFailed(_clock.UtcNow, $"Process exited with code {process.ExitCode}.", process.ExitCode);
            await _jobRunRepository.UpdateAsync(run,ct);
        }
        catch (Exception e)
        {
            throw;
        }
        finally
        {
            _runningJobRegistry.TryRemove(runId, out _);
            linkedCts.Dispose();
            process.Dispose();
        }
    }

    public async Task CancelAsync(JobRunId runId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var run = await _jobRunRepository.GetByIdAsync(runId, ct);
        ArgumentNullException.ThrowIfNull(run);
        // cancel le vrai travail
        
        run.MarkCancelled(_clock.UtcNow,"Cancelled");
        CancelledRunIds.Add(runId);
        await _jobRunRepository.UpdateAsync(run,ct);
    }
}