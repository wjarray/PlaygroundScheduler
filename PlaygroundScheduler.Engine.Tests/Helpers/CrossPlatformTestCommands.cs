namespace PlaygroundScheduler.Engine.Tests.Helpers;

public static class CrossPlatformTestCommands
{
    public static string SleepForCancellationScenario()
        => OperatingSystem.IsWindows()
            ? "powershell -Command \"Start-Sleep -Seconds 10\""
            : "sleep 10";
    
    public static string SleepShort() =>
        OperatingSystem.IsWindows()
            ? "powershell -Command \"Start-Sleep -Milliseconds 500\""
            : "sleep 0.5";

    public static string Succeed() =>
        OperatingSystem.IsWindows()
            ? "powershell -Command \"exit 0\""
            : "sh -c 'exit 0'";

    public static string Fail() =>
        OperatingSystem.IsWindows()
            ? "powershell -Command \"exit 1\""
            : "sh -c 'exit 1'";
    
    public static string HelloWorld() =>
        "echo Hello World";
    
    public static string FailWithStdErr() =>
        OperatingSystem.IsWindows()
            ? "powershell -Command \"[Console]::Error.WriteLine('boom'); exit 1\""
            : "sh -c 'echo boom 1>&2; exit 1'";
}