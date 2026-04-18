using System.Diagnostics;

namespace PlaygroundScheduler.Engine.Infra.Runner;

public sealed class RunningJobHandle
{
    public Process Process { get; }
    public CancellationTokenSource Cancellation { get; }

    public RunningJobHandle(Process process, CancellationTokenSource cancellation)
    {
        Process = process;
        Cancellation = cancellation;
    }
}