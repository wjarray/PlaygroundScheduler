using System.Diagnostics;
using PlaygroundScheduler.Application.Ports;

namespace PlaygroundScheduler.Infrastructure.Runner.Ports;

public sealed class WindowsProcessStartInfoFactory : IProcessStartInfoFactory
{
    public ProcessStartInfo Create(string commandLine)
        => new()
        {
            FileName = "cmd.exe",
            Arguments = $"/C {commandLine}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
}