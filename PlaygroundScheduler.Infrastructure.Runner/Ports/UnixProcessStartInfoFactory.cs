using System.Diagnostics;
using PlaygroundScheduler.Application.Ports;

namespace PlaygroundScheduler.Infrastructure.Runner.Ports;

public sealed class UnixProcessStartInfoFactory : IProcessStartInfoFactory
{
    public ProcessStartInfo Create(string commandLine)
        => new()
        {
            FileName = "/bin/bash",
            Arguments = $"-lc \"{commandLine}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
       
}