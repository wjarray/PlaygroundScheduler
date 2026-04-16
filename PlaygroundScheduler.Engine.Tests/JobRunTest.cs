using PlaygroundScheduler.Engine.Domain.Identity;

namespace PlaygroundScheduler.Engine.Tests;

public class JobRunTest
{
    [Fact]
    public void NEW_RUN_SHOULD_BE_PENDING()
    {
        var createdAt = new DateTimeOffset(2016, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var run = new JobRun(JobRunId.New(), JobDefinitionId.New(), createdAt);
        Assert.Equal(RunStatus.Pending, run.RunStatus);
        Assert.Equal(createdAt, run.CreatedAt);
        Assert.Null(run.StartedAt);
        Assert.Null(run.ErrorMessage);
        Assert.Null(run.EndedAt);
        Assert.Null(run.ExitCode);
    }

    [Fact]
    public void STARTED_RUN_SHOULD_BE_RUNNING()
    {
        var createdAt = new DateTimeOffset(2016, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var run = new JobRun(JobRunId.New(), JobDefinitionId.New(), createdAt);
        var startedAt = createdAt.AddDays(1);
        run.MarkRunning(startedAt);
        Assert.Equal(RunStatus.Running, run.RunStatus);
        Assert.Equal(createdAt, run.CreatedAt);
        Assert.Equal(startedAt, run.StartedAt);
        Assert.Null(run.ErrorMessage);
        Assert.Null(run.EndedAt);
        Assert.Null(run.ExitCode);
    }

    [Fact]
    public void THROW_IF_STARTED_RUN_BEFORE_CREATED_AT()
    {
        var createdAt = new DateTimeOffset(2016, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var run = new JobRun(JobRunId.New(), JobDefinitionId.New(), createdAt);
        var startedAt = createdAt.AddDays(-1);
        Assert.Throws<InvalidOperationException>(() => run.MarkRunning(startedAt));
    }

    [Theory]
    [InlineData(RunStatus.Cancelled)]
    [InlineData(RunStatus.Running)]
    [InlineData(RunStatus.Failed)]
    [InlineData(RunStatus.Succeeded)]
    public void ONLY_PENDING_RUN_SHOULD_BE_STARTED(RunStatus pStatus)
    {
        var createdAt = new DateTimeOffset(2016, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var run = CreateRunInState(pStatus, createdAt);
        Assert.Throws<InvalidOperationException>(() => run.MarkRunning(createdAt.AddMinutes(1)));
    }

    [Theory]
    [InlineData(RunStatus.Cancelled)]
    [InlineData(RunStatus.Pending)]
    [InlineData(RunStatus.Failed)]
    [InlineData(RunStatus.Succeeded)]
    public void ONLY_RUNNING_RUN_CAN_BE_MARKED_SUCCEEDED(RunStatus pStatus)
    {
        var createdAt = new DateTimeOffset(2016, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var run = CreateRunInState(pStatus, createdAt);
        var anyTime = createdAt.AddHours(2);
        Assert.Throws<InvalidOperationException>(() => run.MarkSucceeded(anyTime, 1));
    }

    [Theory]
    [InlineData(RunStatus.Cancelled)]
    [InlineData(RunStatus.Pending)]
    [InlineData(RunStatus.Failed)]
    [InlineData(RunStatus.Succeeded)]
    public void ONLY_RUNNING_RUN_SHOULD_BE_FAILED(RunStatus pStatus)
    {
        var createdAt = new DateTimeOffset(2016, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var run = CreateRunInState(pStatus, createdAt);
        var anyTime = createdAt.AddHours(2);
        Assert.Throws<InvalidOperationException>(() => run.MarkFailed(anyTime, "Error", -1));
    }

    [Fact]
    public void RUNNING_RUN_CAN_BE_MARKED_SUCCEEDED()
    {
        var createdAt = new DateTimeOffset(2016, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var run = CreateRunInState(RunStatus.Running, createdAt);
        var startedAt = createdAt.AddHours(2);
        var anyTime = createdAt.AddHours(2);
        run.MarkSucceeded(anyTime, 1);
        
        Assert.Equal(RunStatus.Succeeded, run.RunStatus);
    }

    [Fact]
    public void RUNNING_RUN_CAN_BE_MARKED_FAILED()
    {
        var createdAt = new DateTimeOffset(2016, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var run = CreateRunInState(RunStatus.Running, createdAt);
        var startedAt = createdAt.AddHours(2);
        var anyTime = createdAt.AddHours(2);
        run.MarkFailed(anyTime,"Error" ,-1);
        Assert.Equal(RunStatus.Failed, run.RunStatus);
    }

    [Fact]
    public void PENDING_RUN_CAN_BE_MARKED_CANCELLED()
    {
        var createdAt = new DateTimeOffset(2016, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var run = CreateRunInState(RunStatus.Pending, createdAt);
        var startedAt = createdAt.AddHours(2);
        var anyTime = createdAt.AddHours(2);
        run.MarkCancelled(anyTime,"Error");
        Assert.Equal(RunStatus.Cancelled, run.RunStatus);
    }

    [Fact]
    public void RUNNING_RUN_CAN_BE_MARKED_CANCELLED()
    {
        var createdAt = new DateTimeOffset(2016, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var run = CreateRunInState(RunStatus.Running, createdAt);
        var startedAt = createdAt.AddHours(2);
        var anyTime = createdAt.AddHours(2);
        run.MarkCancelled(anyTime,"Error");
        Assert.Equal(RunStatus.Cancelled, run.RunStatus);
    }
    
    private JobRun CreateRunInState(RunStatus runStatus, DateTimeOffset pCreatedAt)
    {
        var run = new JobRun(JobRunId.New(), JobDefinitionId.New(), pCreatedAt);
        var startedAt = pCreatedAt.AddHours(1);
        var endedAt = startedAt.AddHours(1);
        switch (runStatus)
        {
            case RunStatus.Pending: break;
            case RunStatus.Running: run.MarkRunning(startedAt); break;
            case RunStatus.Cancelled:
                run.MarkRunning(startedAt);
                run.MarkCancelled(endedAt, "Cancelled");
                break;
            case RunStatus.Failed:
                run.MarkRunning(startedAt);
                run.MarkFailed(endedAt, "Failed", -1);
                break;
            case RunStatus.Succeeded:
                run.MarkRunning(startedAt);
                run.MarkSucceeded(endedAt, 1);
                break;
            default: throw new ArgumentOutOfRangeException(nameof(runStatus), runStatus, null);
        }

        return run;
    }
}