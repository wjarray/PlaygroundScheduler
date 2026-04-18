using PlaygroundScheduler.Domain.Identity;
using PlaygroundScheduler.Infrastructure.Runner.Runner;

namespace PlaygroundScheduler.Infrastructure.Runner.Registry;

public interface IRunningJobRegistry
{
    bool TryRegister(JobRunId runId, RunningJobHandle handle);
    bool TryGet(JobRunId runId, out RunningJobHandle? handle);
    bool TryRemove(JobRunId runId, out RunningJobHandle? handle);
}

