using System.Diagnostics;

namespace PlaygroundScheduler.Application.Ports;

public interface IProcessStartInfoFactory
{
    ProcessStartInfo Create(string commandLine);
}