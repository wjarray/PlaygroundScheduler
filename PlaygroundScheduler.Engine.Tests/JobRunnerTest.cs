using PlaygroundScheduler.Application;
using PlaygroundScheduler.Domain.Identity;
using PlaygroundScheduler.Engine.Tests.Helpers;
using PlaygroundScheduler.Infrastructure.Runner.Ports;
using PlaygroundScheduler.Infrastructure.Runner.Registry;
using PlaygroundScheduler.Infrastructure.Runner.Repository;
using PlaygroundScheduler.Infrastructure.Runner.Runner;
using PlaygroundScheduler.Infrastructure.Runner.Store;

namespace PlaygroundScheduler.Engine.Tests;

public class JobRunnerTest
{

    [Theory]
    [InlineData(RunStatus.Cancelled)]
    [InlineData(RunStatus.Running)]
    [InlineData(RunStatus.Failed)]
    [InlineData(RunStatus.Succeeded)]
    public async Task THROWS_IF_RUNNER_STARTS_NON_PENDING_RUN(RunStatus status)
    {
        // ARRANGE
        // Create job definition with id available, in repo
        var jobDefinitionId = JobDefinitionId.New();
        var command = CrossPlatformTestCommands.Succeed();
        var jobDefinition = new JobDefinition(jobDefinitionId, "Success", command, 0);
        var definitionRepo = new InMemoryJobDefinitionRepository([jobDefinition]);
        var createdAt = new DateTimeOffset(2016, 01, 01, 0, 0, 0, TimeSpan.Zero);

        var run = CreateRunInState(jobDefinitionId,status,createdAt);
        // Create run repo empty
        var runRepo = new InMemoryJobRunRepository([run]);
        var clock = new FakeClock()
        {
            UtcNow = DateTimeOffset.UtcNow
        };

        var registry = new InMemoryRunningJobRegistry();
        // Create job runner
        var outputStore = new InMemoryJobRunOutputStore();
        var processStartInfoFactory = ProcessStartInfoFactorySelector.CreateDefault();
        var runner = new LocalJobRunner(runRepo, definitionRepo, clock, registry, outputStore,processStartInfoFactory);


        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.StartAsync(run.Id));
    }
    
    [Theory]
    [InlineData(RunStatus.Cancelled)]
    [InlineData(RunStatus.Failed)]
    [InlineData(RunStatus.Succeeded)]
    public async Task THROWS_IF_RUNNER_CANCELS_TERMINAL_RUN(RunStatus status)
        {
            // ARRANGE
            // Create job definition with id available, in repo
            var jobDefinitionId = JobDefinitionId.New();
            var command = CrossPlatformTestCommands.Succeed();
            
            var jobDefinition = new JobDefinition(jobDefinitionId, "Success", command, 0);
            var definitionRepo = new InMemoryJobDefinitionRepository([jobDefinition]);
            var createdAt = new DateTimeOffset(2016, 01, 01, 0, 0, 0, TimeSpan.Zero);
    
            var run = CreateRunInState(jobDefinitionId,status,createdAt);
            // Create run repo empty
            var runRepo = new InMemoryJobRunRepository([run]);
            var clock = new FakeClock()
            {
                UtcNow = DateTimeOffset.UtcNow
            };
    
            var registry = new InMemoryRunningJobRegistry();
            // Create job runner
            var outputStore = new InMemoryJobRunOutputStore();
            var processStartInfoFactory = ProcessStartInfoFactorySelector.CreateDefault();
            var runner = new LocalJobRunner(runRepo, definitionRepo, clock, registry, outputStore, processStartInfoFactory);
    
            await Assert.ThrowsAsync<InvalidOperationException>(() => runner.CancelAsync(run.Id));
        }
    
    [Fact]
    public async Task REGISTRY_SHOULD_BE_EMPTY_WHEN_NO_RUNNING_RUN()
    {
        // ARRANGE
        // Create job definition with id available, in repo
        var jobDefinitionId = JobDefinitionId.New();
        var command = CrossPlatformTestCommands.Succeed();
        
        var jobDefinition = new JobDefinition(jobDefinitionId, "Success", command, 0);
        var definitionRepo = new InMemoryJobDefinitionRepository([jobDefinition]);
        var createdAt = new DateTimeOffset(2016, 01, 01, 0, 0, 0, TimeSpan.Zero);
    
        var run = CreateRunInState(jobDefinitionId,RunStatus.Pending,createdAt);
        // Create run repo empty
        var runRepo = new InMemoryJobRunRepository([run]);
        var clock = new FakeClock()
        {
            UtcNow = DateTimeOffset.UtcNow
        };
    
        var registry = new InMemoryRunningJobRegistry();
        var outputStore = new InMemoryJobRunOutputStore();
        
        // Create job runnerx
        var processStartInfoFactory = ProcessStartInfoFactorySelector.CreateDefault();
        var runner = new LocalJobRunner(runRepo, definitionRepo, clock, registry, outputStore, processStartInfoFactory);
        CancellationToken ct = CancellationToken.None;
        var startedRun= runner.StartAsync(run.Id,ct);

        await WaitUntilAsync(async () =>
        {
            var current = await runRepo.GetByIdAsync(run.Id,ct);
            var runInRegistry = registry.TryGet(run.Id, out _);
            Assert.True(runInRegistry);
            return current?.RunStatus == RunStatus.Running;
        }, TimeSpan.FromSeconds(2));
        
        await runner.CancelAsync(run.Id, ct);

        var isRunStillInRegistry = registry.TryGet(run.Id, out _);
        Assert.False(isRunStillInRegistry);
    }
    
    [Fact]
    public async Task LOCAL_RUNNER_START_SHOULD_NOT_OVERWRITE_CANCELLED_STATUS_AFTER_PROCESS_EXIT()
    {
        var jobDefinitionId = JobDefinitionId.New();
        var command = CrossPlatformTestCommands.SleepForCancellationScenario();
        var jobDefinition = new JobDefinition(jobDefinitionId, "Sleep", command, 0);
        var definitionRepo = new InMemoryJobDefinitionRepository([jobDefinition]);

        var createdAt = new DateTimeOffset(2016, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var run = CreateRunInState(jobDefinitionId, RunStatus.Pending, createdAt);
        var runRepo = new InMemoryJobRunRepository([run]);

        var clock = new FakeClock
        {
            UtcNow = DateTimeOffset.UtcNow
        };

        var registry = new InMemoryRunningJobRegistry();
        var outputStore = new InMemoryJobRunOutputStore();
        var processStartInfoFactory = ProcessStartInfoFactorySelector.CreateDefault();
        var runner = new LocalJobRunner(runRepo, definitionRepo, clock, registry, outputStore, processStartInfoFactory);

        var ct = CancellationToken.None;
        var startTask = runner.StartAsync(run.Id, ct);

        await WaitUntilAsync(async () =>
        {
            var current = await runRepo.GetByIdAsync(run.Id, ct);
            return current?.RunStatus == RunStatus.Running;
        }, TimeSpan.FromSeconds(2));

        var inRegistry = registry.TryGet(run.Id, out _);
        Assert.True(inRegistry);

        await runner.CancelAsync(run.Id, ct);
        await startTask;

        var refreshedRun = await runRepo.GetByIdAsync(run.Id, ct);

        Assert.NotNull(refreshedRun);
        Assert.Equal(RunStatus.Cancelled, refreshedRun!.RunStatus);
    }
    
    [Fact]
    public async Task DOUBLE_START_SHOULD_NOT_RUN_THE_LAST_ATTEMPT()
    {
        var jobDefinitionId = JobDefinitionId.New();
        var command = CrossPlatformTestCommands.SleepForCancellationScenario();

        var jobDefinition = new JobDefinition(jobDefinitionId, "Sleep", command, 0);
        var definitionRepo = new InMemoryJobDefinitionRepository([jobDefinition]);

        var createdAt = new DateTimeOffset(2016, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var run = CreateRunInState(jobDefinitionId, RunStatus.Pending, createdAt);
        var runRepo = new InMemoryJobRunRepository([run]);

        var clock = new FakeClock
        {
            UtcNow = DateTimeOffset.UtcNow
        };

        var registry = new InMemoryRunningJobRegistry();
        var outputStore = new InMemoryJobRunOutputStore();
        var processStartInfoFactory = ProcessStartInfoFactorySelector.CreateDefault();

        var runner = new LocalJobRunner(runRepo, definitionRepo, clock, registry, outputStore,processStartInfoFactory);

        var ct = CancellationToken.None;
        var startTask = runner.StartAsync(run.Id, ct);

        await WaitUntilAsync(async () =>
        {
            var current = await runRepo.GetByIdAsync(run.Id, ct);
            return current?.RunStatus == RunStatus.Running;
        }, TimeSpan.FromSeconds(2));
        
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => runner.StartAsync(run.Id, ct));
    }

    [Fact]
    public async Task SUCCEEDED_JOB_SHOULD_STORE_OUTPUT_RESULT_IN_STORE()
    {
        var jobDefinitionId = JobDefinitionId.New();
        var command = CrossPlatformTestCommands.HelloWorld();
        
        var jobDefinition = new JobDefinition(jobDefinitionId, "Succeed", command, 0);
        var definitionRepo = new InMemoryJobDefinitionRepository([jobDefinition]);

        var createdAt = new DateTimeOffset(2016, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var run = CreateRunInState(jobDefinitionId, RunStatus.Pending, createdAt);
        var runRepo = new InMemoryJobRunRepository([run]);

        var clock = new FakeClock
        {
            UtcNow = DateTimeOffset.UtcNow
        };

        var registry = new InMemoryRunningJobRegistry();
        var outputStore = new InMemoryJobRunOutputStore();
        var processStartInfoFactory = ProcessStartInfoFactorySelector.CreateDefault();
        var runner = new LocalJobRunner(runRepo, definitionRepo, clock, registry, outputStore,processStartInfoFactory);

        var ct = CancellationToken.None;
        
        await runner.StartAsync(run.Id, ct);

        var output = await outputStore.GetByRunIdAsync(run.Id, ct);

        Assert.NotNull(output);
        Assert.Equal(output.RunId, run.Id);
        Assert.Contains("Hello World", output.StdOutput);
        Assert.Empty(output.StdErr);
    }
    
    [Fact]
    public async Task FAILED_JOB_SHOULD_STORE_OUTPUT_RESULT_IN_STORE()
    {
        var jobDefinitionId = JobDefinitionId.New();
        var command = CrossPlatformTestCommands.FailWithStdErr();
        
        var jobDefinition = new JobDefinition(jobDefinitionId, "Failed",command, 0);
        var definitionRepo = new InMemoryJobDefinitionRepository([jobDefinition]);

        var createdAt = new DateTimeOffset(2016, 01, 01, 0, 0, 0, TimeSpan.Zero);
        var run = CreateRunInState(jobDefinitionId, RunStatus.Pending, createdAt);
        var runRepo = new InMemoryJobRunRepository([run]);

        var clock = new FakeClock
        {
            UtcNow = DateTimeOffset.UtcNow
        };

        var registry = new InMemoryRunningJobRegistry();
        var outputStore = new InMemoryJobRunOutputStore();
        var processStartInfoFactory = ProcessStartInfoFactorySelector.CreateDefault();
        var runner = new LocalJobRunner(runRepo, definitionRepo, clock, registry, outputStore,processStartInfoFactory);

        var ct = CancellationToken.None;
        
        await runner.StartAsync(run.Id, ct);

        var output = await outputStore.GetByRunIdAsync(run.Id, ct);

        Assert.NotNull(output);
        Assert.Equal(output.RunId, run.Id);
        Assert.Empty(output.StdOutput);
        Assert.NotEmpty(output.StdErr);
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
    
    private JobRun CreateRunInState(JobDefinitionId jobDefinitionId,RunStatus runStatus, DateTimeOffset pCreatedAt)
    {
        var run = new JobRun(JobRunId.New(), jobDefinitionId, pCreatedAt);
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