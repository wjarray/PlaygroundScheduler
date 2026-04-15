using System.Diagnostics;
using PlaygroundScheduler.Engine.Domain.Identity;
using PlaygroundScheduler.Engine.Runner;

namespace PlaygroundScheduler.Engine.Registry;

public interface IRunningJobRegistry
{
    bool TryRegister(JobRunId runId, RunningJobHandle handle);
    bool TryGet(JobRunId runId, out RunningJobHandle? handle);
    bool TryRemove(JobRunId runId, out RunningJobHandle? handle);
}

