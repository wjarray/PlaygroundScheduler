using PlaygroundScheduler.Engine.Domain.Identity;
using PlaygroundScheduler.Engine.Infra.Runner;

namespace PlaygroundScheduler.Engine.Infra.Registry;

public sealed class InMemoryRunningJobRegistry : IRunningJobRegistry
{
    private readonly Dictionary<JobRunId, RunningJobHandle> _handles = new();

    public bool TryRegister(JobRunId runId, RunningJobHandle handle)
    {
        return _handles.TryAdd(runId, handle);
    }

    public bool TryGet(JobRunId runId, out RunningJobHandle? handle)
    {
        var found = _handles.TryGetValue(runId, out var value);
        handle = value;
        return found;
    }
    
    public bool TryRemove(JobRunId runId, out RunningJobHandle? handle)
    {
        if (_handles.TryGetValue(runId, out var value))
        {
            _handles.Remove(runId);
            handle = value;
            return true;
        }

        handle = null;
        return false;
    }
}