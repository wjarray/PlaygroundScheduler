using System.ComponentModel;

namespace PlaygroundScheduler.Domain.Identity;


public sealed class JobRun
{
    public JobRunId Id { get; }
    public JobDefinitionId JobDefinitionId { get; }
    public RunStatus RunStatus { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public int? ExitCode { get; private set; }
    public string? ErrorMessage { get; private set; }

    public bool IsTerminal => RunStatus is RunStatus.Cancelled or RunStatus.Failed or RunStatus.Succeeded;

    public JobRun(JobRunId id, JobDefinitionId jobDefinitionId, DateTimeOffset createdAt)
    {
        if (id.Value == Guid.Empty)
            throw new InvalidEnumArgumentException($"{nameof(id)} cannot be empty");
        if (jobDefinitionId.Value == Guid.Empty)
            throw new InvalidEnumArgumentException($"{nameof(id)} cannot be empty");
        
        RunStatus = RunStatus.Pending;
        Id = id;
        JobDefinitionId = jobDefinitionId;
        CreatedAt = createdAt;
    }

    public void MarkRunning(DateTimeOffset pStart)
    {
        if (RunStatus != RunStatus.Pending)
            throw new InvalidOperationException($"Invalid RunStatus, can only start pending run. STATUS = {RunStatus}");

        if (pStart < CreatedAt)
            throw new InvalidOperationException($"Invalid Started At, cannot be before Created At");

        this.StartedAt = pStart; 
        this.RunStatus = RunStatus.Running;
    }

    public void MarkFailed(DateTimeOffset pFailedAt, string? pErrorMessage,int pExitCode)
    {
        EnsureNotBeforeCreated(pFailedAt);
        EnsureNotBeforeStarted(pFailedAt);
        EnsureRunning();
        
        if (string.IsNullOrEmpty(pErrorMessage))
            throw new InvalidEnumArgumentException($"{nameof(pErrorMessage)} cannot be null or empty");
      
        this.EndedAt = pFailedAt;
        this.ErrorMessage = pErrorMessage;
        this.ExitCode = pExitCode;
        this.RunStatus = RunStatus.Failed;
    }

    public void MarkCancelled(DateTimeOffset pCancelledAt, string? pErrorMessage)
    {
        EnsureNotBeforeCreated(pCancelledAt);
        
        if (RunStatus is not (RunStatus.Pending or RunStatus.Running))
            throw new InvalidOperationException($"Only pending or running runs can be cancelled. Current status: {RunStatus}");
        this.EndedAt = pCancelledAt;
        this.ErrorMessage = pErrorMessage ?? "Empty cancelled message";
        this.RunStatus = RunStatus.Cancelled;
        this.ExitCode = -2;
    }

    public void MarkSucceeded(DateTimeOffset pSucceededAt,int pExitCode)
    {
        EnsureNotBeforeCreated(pSucceededAt);
        EnsureNotBeforeStarted(pSucceededAt);
        EnsureRunning();
        if (pSucceededAt < StartedAt)
            throw new InvalidOperationException($"Invalid MarkSucceeded At, cannot be before Created At");
        this.EndedAt = pSucceededAt;
        this.RunStatus = RunStatus.Succeeded;
        this.ExitCode = pExitCode;
    }

    private void EnsureNotBeforeCreated(DateTimeOffset when)
    {
        if (when < CreatedAt)
            throw new InvalidOperationException("Date cannot be before CreatedAt.");
    }

    private void EnsureNotBeforeStarted(DateTimeOffset when)
    {
        if (StartedAt is null)
            throw new InvalidOperationException("Run has not started.");
        if (when < StartedAt.Value)
            throw new InvalidOperationException("Date cannot be before StartedAt.");
    }

    private void EnsureRunning()
    {
        if (RunStatus != RunStatus.Running)
            throw new InvalidOperationException($"Run must be running. Current status: {RunStatus}");
    }
}