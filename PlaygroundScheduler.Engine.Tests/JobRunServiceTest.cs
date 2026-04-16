using System.Diagnostics.Tracing;
using PlaygroundScheduler.Engine.Domain.Identity;
using PlaygroundScheduler.Engine.Registry;
using PlaygroundScheduler.Engine.Repository;
using PlaygroundScheduler.Engine.Runner;
using PlaygroundScheduler.Engine.Services;

namespace PlaygroundScheduler.Engine.Tests;

public class JobRunServiceTest
{
    [Fact]
    public async Task CREATE_RUN_SHOULD_CREATE_PENDING_RUN_WHEN_DEFINITION_EXISTS()
    {
        // ARRANGE
        // Create job definition with id available, in repo
        var jobDefinitionId = JobDefinitionId.New();
        var jobDefinition = new JobDefinition(jobDefinitionId, "Hello World", "echo 'Hello World", 0);
        var definitionRepo = new InMemoryJobDefinitionRepository([jobDefinition]);
        
        // Create run repo empty
        var runRepo = new InMemoryJobRunRepository();
        var clock = new FakeClock()
        {
            UtcNow = new DateTimeOffset(2026, 4, 14, 10, 0, 0, TimeSpan.Zero)
        };
        
        // Create job runner
        var runner = new FakeLocalJobRunner();
        // Create job runner service
        var jobRunnerService = new JobRunService(definitionRepo,runRepo,runner,clock);
        
        // ACT
        // Create run
        var runId = await jobRunnerService.CreateRunAsync(jobDefinition.DefinitionId);
        // Retrieve created run
        var run = await runRepo.GetByIdAsync(runId);
        
        
        // ASSERT should be in pending status
        Assert.NotNull(run);
        Assert.Equal(jobDefinitionId, run.JobDefinitionId);
        Assert.Equal(RunStatus.Pending, run.RunStatus);
        Assert.Equal(clock.UtcNow, run.CreatedAt);
    }

    [Fact]
    public async Task CREATE_RUN_SHOULD_THROW_WHEN_DEFINITION_DOES_NOT_EXIST()
    {
        // ARRANGE
        // Create empty job repo and empty run repo
        var definitionRepo = new InMemoryJobDefinitionRepository();
        var runRepo = new InMemoryJobRunRepository();
        var clock = new FakeClock()
        {
            UtcNow = new DateTimeOffset(2026, 4, 14, 10, 0, 0, TimeSpan.Zero)
        };
        
        // Create runner
        var runner = new FakeLocalJobRunner();
        // Create service
        var jobRunnerService = new JobRunService(definitionRepo, runRepo, runner, clock);
        
        // ACT & ASSERT 
        // Create non existing JobDefinition
        var definition = JobDefinitionId.New();
        // Should throw because doesn't exists in repo
        await Assert.ThrowsAsync<InvalidOperationException>(() =>  jobRunnerService.CreateRunAsync(definition));
    }

    //Start
    [Fact]
    public async Task START_RUN_SHOULD_CALL_RUNNER_WHEN_RUN_IS_PENDING()
    {
        // Arrange
        // Create job definition available in definition repo
        var jobDefinition = new JobDefinition(JobDefinitionId.New(),"NAME : THIS RUN IS PENDING UNTIL RUNNER STARTS IT","Pending command line",0);
        var definitionRepo = new InMemoryJobDefinitionRepository([jobDefinition]);

        // Create a pending run available in run repo
        var createdAt = new DateTimeOffset(2026, 4, 14, 9, 0, 0, TimeSpan.Zero);
        var pendingRun = new JobRun(JobRunId.New(),jobDefinition.DefinitionId,createdAt);
        var runRepo = new InMemoryJobRunRepository([pendingRun]);
        
        var clock = new FakeClock()
        {
            UtcNow = new DateTimeOffset(2026, 4, 14, 10, 0, 0, TimeSpan.Zero)
        };
        
        // create fake runner that contains a private list of started and cancelled run id.
        var runner = new FakeLocalJobRunner();
        
        // create service
        var jobRunnerService = new JobRunService(definitionRepo, runRepo, runner, clock);
        
        // ACT
        // start the pending run avaialble in repo
        await jobRunnerService.StartRunAsync(pendingRun.Id);
        
        // service should call the runner ( and fill the repo ) if the run is pending
        Assert.Single(runner.StartedRunIds);
        Assert.Equal(pendingRun.Id, runner.StartedRunIds[0]);
    }

    [Fact]
    public async Task START_RUN_SHOULD_THROW_WHEN_RUN_DOES_NOT_EXIST()
    {
        // ASSERT
        // Create job definition
        var jobDefinition = new JobDefinition(JobDefinitionId.New(),
            "NAME",
            "echo 'Hello World",
            0);

        var jobRepo = new InMemoryJobDefinitionRepository([jobDefinition]);
        
        var createdAt = new DateTimeOffset(2026, 4, 14, 9, 0, 0, TimeSpan.Zero);
        var pendingRun = new JobRun(JobRunId.New(),jobDefinition.DefinitionId,createdAt);
        
        // empty on purpose, run doesn't exist
        var runRepo = new InMemoryJobRunRepository();
        var clock = new FakeClock()
        {
            UtcNow = new DateTimeOffset(2026, 4, 14, 10, 0, 0, TimeSpan.Zero)
        };
        var runner = new FakeLocalJobRunner();

        var jobRunnerService = new JobRunService(jobRepo,runRepo,runner,clock);
        await Assert.ThrowsAsync<InvalidOperationException>(() => jobRunnerService.StartRunAsync(pendingRun.Id));
    }

    [Theory]
    [InlineData(RunStatus.Cancelled)]
    [InlineData(RunStatus.Running)]
    [InlineData(RunStatus.Failed)]
    [InlineData(RunStatus.Succeeded)]
    public async Task START_RUN_SHOULD_THROW_WHEN_RUN_IS_NOT_PENDING(RunStatus status)
    {
        // ASSERT
        // Create job definition
        var jobDefinition = new JobDefinition(JobDefinitionId.New(),
            "Hi world",
            "echo 'Hello World'",
            0);

        var jobRepo = new InMemoryJobDefinitionRepository([jobDefinition]);
        var createdAt = new DateTimeOffset(2026, 4, 14, 9, 0, 0, TimeSpan.Zero);
        var inStatusRun = CreateRunInState(JobRunId.New(),jobDefinition.DefinitionId,status,createdAt,null,null);
        
        // empty on purpose, run doesn't exist
        var runRepo = new InMemoryJobRunRepository([inStatusRun]);
        var clock = new FakeClock()
        {
            UtcNow = new DateTimeOffset(2026, 4, 14, 10, 0, 0, TimeSpan.Zero)
        };
        var runner = new FakeLocalJobRunner();

        var jobRunnerService = new JobRunService(jobRepo,runRepo,runner,clock);
        await Assert.ThrowsAsync<InvalidOperationException>(() => jobRunnerService.StartRunAsync(inStatusRun.Id));
    }

    // Cancel
    [Fact]
    public async Task CANCEL_RUN_SHOULD_CANCEL_DIRECTLY_WHEN_RUN_IS_PENDING()
    {
        // ASSERT
        // Create job definition
        var jobDefinition = new JobDefinition(JobDefinitionId.New(),
            "NAME",
            "echo 'Hello World",
            0);

        var jobRepo = new InMemoryJobDefinitionRepository([jobDefinition]);
        var createdAt = new DateTimeOffset(2026, 4, 14, 9, 0, 0, TimeSpan.Zero);
        var pendingRun = CreateRunInState(JobRunId.New(),jobDefinition.DefinitionId,RunStatus.Pending,createdAt,null,null);
        
        var runRepo = new InMemoryJobRunRepository([pendingRun]);
        var clock = new FakeClock()
        {
            UtcNow = new DateTimeOffset(2026, 4, 14, 10, 0, 0, TimeSpan.Zero)
        };
        var runner = new FakeLocalJobRunner();

        var jobRunnerService = new JobRunService(jobRepo,runRepo,runner,clock);
        await jobRunnerService.CancelRunAsync(pendingRun.Id);

        var cancelledRun = await jobRunnerService.GetRunAsync(pendingRun.Id);
        Assert.NotNull(cancelledRun);
        Assert.Equal(cancelledRun.Id,pendingRun.Id);
    }

    [Fact]
    public async Task CANCEL_RUN_SHOULD_CALL_RUNNER_WHEN_RUN_IS_RUNNING()
    {
        // ASSERT
        // Create job definition
        var jobDefinition = new JobDefinition(JobDefinitionId.New(),
            "NAME",
            "echo 'Hello World",
            0);

        var jobRepo = new InMemoryJobDefinitionRepository([jobDefinition]);
        var createdAt = new DateTimeOffset(2026, 4, 14, 9, 0, 0, TimeSpan.Zero);
        var startedAt = createdAt.AddHours(1);
        var startedRun = CreateRunInState(JobRunId.New(),jobDefinition.DefinitionId,RunStatus.Running,createdAt,startedAt,null);
        
        var runRepo = new InMemoryJobRunRepository([startedRun]);
        var clock = new FakeClock()
        {
            UtcNow = new DateTimeOffset(2026, 4, 14, 10, 0, 0, TimeSpan.Zero)
        };
        var runner = new FakeLocalJobRunner();

        var jobRunnerService = new JobRunService(jobRepo,runRepo,runner,clock);
        await jobRunnerService.CancelRunAsync(startedRun.Id);
        var cancelledRun = await jobRunnerService.GetRunAsync(startedRun.Id);
        
        Assert.Single(runner.CancelledRunIds);
        Assert.Equal(runner.CancelledRunIds[0],cancelledRun!.Id);
    }

    [Theory]
    [InlineData(RunStatus.Cancelled)]
    [InlineData(RunStatus.Failed)]
    [InlineData(RunStatus.Succeeded)]
    public async Task CANCEL_RUN_SHOULD_THROW_WHEN_RUN_IS_TERMINAL(RunStatus terminalStatus)
    {
        // ASSERT
        // Create job definition
        var jobDefinition = new JobDefinition(JobDefinitionId.New(),
            "NAME",
            "echo 'Hello World",
            0);

        var jobRepo = new InMemoryJobDefinitionRepository([jobDefinition]);
        var createdAt = new DateTimeOffset(2026, 4, 14, 9, 0, 0, TimeSpan.Zero);
        var inStatusRun = CreateRunInState(JobRunId.New(),jobDefinition.DefinitionId,terminalStatus,createdAt,null,null);
        
        // empty on purpose, run doesn't exist
        var runRepo = new InMemoryJobRunRepository([inStatusRun]);
        var clock = new FakeClock()
        {
            UtcNow = new DateTimeOffset(2026, 4, 14, 10, 0, 0, TimeSpan.Zero)
        };
        var runner = new FakeLocalJobRunner();

        var jobRunnerService = new JobRunService(jobRepo,runRepo,runner,clock);
        await Assert.ThrowsAsync<InvalidOperationException>(() => jobRunnerService.CancelRunAsync(inStatusRun.Id));
    }

    [Theory]
    [InlineData(RunStatus.Cancelled)]
    [InlineData(RunStatus.Running)]
    [InlineData(RunStatus.Failed)]
    [InlineData(RunStatus.Succeeded)]
    [InlineData(RunStatus.Pending)]
    public async Task CANCEL_RUN_SHOULD_THROW_WHEN_RUN_DOES_NOT_EXIST(RunStatus runStatus)
    {
        // ASSERT
        // Create job definition
        var jobDefinition = new JobDefinition(JobDefinitionId.New(),
            "NAME",
            "echo 'Hello World",
            0);

        var jobRepo = new InMemoryJobDefinitionRepository([jobDefinition]);
        var createdAt = new DateTimeOffset(2026, 4, 14, 9, 0, 0, TimeSpan.Zero);
        var inStatusRun = CreateRunInState(JobRunId.New(),jobDefinition.DefinitionId,runStatus,createdAt,null,null);
        
        // empty on purpose, run doesn't exist
        var runRepo = new InMemoryJobRunRepository();
        var clock = new FakeClock()
        {
            UtcNow = new DateTimeOffset(2026, 4, 14, 10, 0, 0, TimeSpan.Zero)
        };
        var runner = new FakeLocalJobRunner();

        var jobRunnerService = new JobRunService(jobRepo,runRepo,runner,clock);
        await Assert.ThrowsAsync<InvalidOperationException>(() => jobRunnerService.CancelRunAsync(inStatusRun.Id));
    }

    // Read 

    [Theory]
    [InlineData(RunStatus.Cancelled)]
    [InlineData(RunStatus.Running)]
    [InlineData(RunStatus.Failed)]
    [InlineData(RunStatus.Succeeded)]
    [InlineData(RunStatus.Pending)]
    public async Task GET_RUN_SHOULD_RETURN_RUN(RunStatus status)
   {
       // ASSERT
       // Create job definition
       var jobDefinition = new JobDefinition(JobDefinitionId.New(),
           "NAME",
           "echo 'Hello World",
           0);

       var jobRepo = new InMemoryJobDefinitionRepository([jobDefinition]);
       var createdAt = new DateTimeOffset(2026, 4, 14, 9, 0, 0, TimeSpan.Zero);
       var runInState = CreateRunInState(JobRunId.New(),jobDefinition.DefinitionId,status,createdAt,null,null);
        
       var runRepo = new InMemoryJobRunRepository([runInState]);
       var clock = new FakeClock()
       {
           UtcNow = new DateTimeOffset(2026, 4, 14, 10, 0, 0, TimeSpan.Zero)
       };
       var runner = new FakeLocalJobRunner();

       var jobRunnerService = new JobRunService(jobRepo,runRepo,runner,clock);

       var finalRun = await jobRunnerService.GetRunAsync(runInState.Id);
       Assert.NotNull(finalRun);
       Assert.Equal(finalRun.Id,runInState.Id);
   }
    
    [Theory]
    [InlineData(RunStatus.Cancelled)]
    [InlineData(RunStatus.Running)]
    [InlineData(RunStatus.Failed)]
    [InlineData(RunStatus.Succeeded)]
    [InlineData(RunStatus.Pending)]
    public async Task LIST_RUNS_SHOULD_RETURN_RUNS_FOR_JOB(RunStatus status)
    {
        
        // Create job definition
        var jobDefinition = new JobDefinition(JobDefinitionId.New(),
            "NAME",
            "echo 'Hello World",
            0);

        var jobRepo = new InMemoryJobDefinitionRepository([jobDefinition]);
        var createdAt = new DateTimeOffset(2026, 4, 14, 9, 0, 0, TimeSpan.Zero);
        
        var runInState = CreateRunInState(JobRunId.New(),jobDefinition.DefinitionId,status,createdAt,null,null);
        
        var runRepo = new InMemoryJobRunRepository([runInState,runInState,runInState,runInState,runInState]);
        var clock = new FakeClock()
        {
            UtcNow = new DateTimeOffset(2026, 4, 14, 10, 0, 0, TimeSpan.Zero)
        };
        var runner = new FakeLocalJobRunner();

        var jobRunnerService = new JobRunService(jobRepo,runRepo,runner,clock);

        var finalRun = await jobRunnerService.ListRunsAsync(jobDefinition.DefinitionId);
        Assert.NotNull(finalRun);
        Assert.Equal(5,finalRun.Count);
    }
    
    [Fact]
    public async Task LOCAL_RUNNER_START_SHOULD_MARK_RUN_AS_SUCCEEDED_WHEN_PROCESS_EXITS_WITH_CODE_0()
    {
        // ARRANGE
        // Create job definition with id available, in repo
        var jobDefinitionId = JobDefinitionId.New();
        var jobDefinition = new JobDefinition(jobDefinitionId, "Hello World", "echo Hello World", 0);
        var definitionRepo = new InMemoryJobDefinitionRepository([jobDefinition]);
        
        // Create run repo empty
        var runRepo = new InMemoryJobRunRepository();
        var clock = new FakeClock()
        {
            UtcNow = DateTimeOffset.UtcNow
        };

        var registry = new InMemoryRunningJobRegistry();
        // Create job runner
        var runner = new LocalJobRunner(runRepo,definitionRepo,clock,registry);
        // Create job runner service
        var jobRunnerService = new JobRunService(definitionRepo,runRepo,runner,clock);
        
        // ACT
        // Create run
        var runId = await jobRunnerService.CreateRunAsync(jobDefinition.DefinitionId);
        
        await jobRunnerService.StartRunAsync(runId);
        // Retrieve created run
        var run = await runRepo.GetByIdAsync(runId);
        
        
        // ASSERT should be in pending status
        Assert.NotNull(run);
        Assert.Equal(jobDefinitionId, run.JobDefinitionId);
        Assert.Equal(clock.UtcNow, run.CreatedAt);
        Assert.Equal(RunStatus.Succeeded,run.RunStatus);
    }
    
    [Fact]
    public async Task LOCAL_RUNNER_START_SHOULD_MARK_RUN_AS_FAILED_WHEN_PROCESS_EXITS_WITH_CODE_NON_0()
    {
        // ARRANGE
        // Create job definition with id available, in repo
        var jobDefinitionId = JobDefinitionId.New();
        var jobDefinition = new JobDefinition(jobDefinitionId, "Hello World", "exit 2", 0);
        var definitionRepo = new InMemoryJobDefinitionRepository([jobDefinition]);
        
        // Create run repo empty
        var runRepo = new InMemoryJobRunRepository();
        var clock = new FakeClock()
        {
            UtcNow = DateTimeOffset.UtcNow
        };

        var registry = new InMemoryRunningJobRegistry();
        // Create job runner
        var runner = new LocalJobRunner(runRepo,definitionRepo,clock,registry);
        // Create job runner service
        var jobRunnerService = new JobRunService(definitionRepo,runRepo,runner,clock);
        
        // ACT
        // Create run
        var runId = await jobRunnerService.CreateRunAsync(jobDefinition.DefinitionId);
        
        await jobRunnerService.StartRunAsync(runId);
        // Retrieve created run
        var run = await runRepo.GetByIdAsync(runId);
        
        
        // ASSERT should be in pending status
        Assert.NotNull(run);
        Assert.Equal(jobDefinitionId, run.JobDefinitionId);
        Assert.Equal(clock.UtcNow, run.CreatedAt);
        Assert.Equal(RunStatus.Failed,run.RunStatus);
        Assert.Equal(2,run.ExitCode);
        
    }

    [Fact]
    public async Task LOCAL_RUNNER_START_SHOULD_MARK_RUN_AS_CANCELLED_WHEN_CANCELLED_IS_FIRED()
    {
        // ARRANGE
        // Create job definition with id available, in repo
        var jobDefinitionId = JobDefinitionId.New();
        var jobDefinition = new JobDefinition(jobDefinitionId, "SLEEP10", "sleep 10", 0);
        var definitionRepo = new InMemoryJobDefinitionRepository([jobDefinition]);
        
        // Create run repo empty
        var runRepo = new InMemoryJobRunRepository();
        var clock = new FakeClock()
        {
            UtcNow = DateTimeOffset.UtcNow
        };

        var registry = new InMemoryRunningJobRegistry();
        // Create job runner
        var runner = new LocalJobRunner(runRepo,definitionRepo,clock,registry);
        // Create job runner service
        var jobRunnerService = new JobRunService(definitionRepo,runRepo,runner,clock);
        
        // ACT
        // Create run
        var runId = await jobRunnerService.CreateRunAsync(jobDefinition.DefinitionId); 
        _ = jobRunnerService.StartRunAsync(runId,CancellationToken.None);
        
        await WaitUntilAsync(async () =>
        {
            var current = await runRepo.GetByIdAsync(runId);
            if (current?.RunStatus == RunStatus.Running)
            {
                _ = jobRunnerService.CancelRunAsync(runId, CancellationToken.None);
                return true;
            }
            return false;
        }, TimeSpan.FromSeconds(2));
        
        var run = await runRepo.GetByIdAsync(runId);
        
        // ASSERT should be in pending status
        Assert.NotNull(run);
        Assert.Equal(jobDefinitionId, run.JobDefinitionId);
        Assert.Equal(RunStatus.Cancelled,run.RunStatus);
    }

    private static async Task WaitUntilAsync(Func<Task<bool>> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            if (await condition())
                return;

            await Task.Delay(50);
        }

        throw new TimeoutException("Condition was not met within the expected timeout.");
    }
    private static JobRun CreateRunInState(JobRunId runId, JobDefinitionId definitionId, RunStatus runStatus, DateTimeOffset pCreatedAt,DateTimeOffset? pStartedAt, DateTimeOffset? pEndedAt)
    {
        var run = new JobRun(runId, definitionId, pCreatedAt);
        var startedAt = pStartedAt ?? pCreatedAt.AddHours(1); 
        var endedAt = pEndedAt ?? startedAt.AddHours(1); 
        
        switch (runStatus)
        {
            case RunStatus.Pending:
                break;
            case RunStatus.Running:
                run.MarkRunning(startedAt);
                break;
            case RunStatus.Cancelled:
                run.MarkRunning(startedAt);
                run.MarkCancelled(endedAt,"Cancelled");
                break;
            case RunStatus.Failed:
                run.MarkRunning(startedAt);
                run.MarkFailed(endedAt,"Failed",-1);
                break;
            case RunStatus.Succeeded:
                run.MarkRunning(startedAt);
                run.MarkSucceeded(endedAt,1);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(runStatus), runStatus, null);
        }

        return run;
    }
}