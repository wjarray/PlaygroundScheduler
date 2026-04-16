using PlaygroundScheduler.Engine.Domain.Identity;
using PlaygroundScheduler.Engine.Infra.Runner;

namespace PlaygroundScheduler.Engine.Infra.Registry;

public interface IRunningJobRegistry
{
    bool TryRegister(JobRunId runId, RunningJobHandle handle);
    bool TryGet(JobRunId runId, out RunningJobHandle? handle);
    bool TryRemove(JobRunId runId, out RunningJobHandle? handle);
}

