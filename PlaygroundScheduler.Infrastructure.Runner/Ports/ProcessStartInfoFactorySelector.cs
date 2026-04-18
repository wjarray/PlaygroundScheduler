using PlaygroundScheduler.Application.Ports;

namespace PlaygroundScheduler.Infrastructure.Runner.Ports;

public static class ProcessStartInfoFactorySelector
{
    public static IProcessStartInfoFactory CreateDefault()
    {
        if (OperatingSystem.IsWindows())
            return new WindowsProcessStartInfoFactory();

        return new UnixProcessStartInfoFactory();
    }
}