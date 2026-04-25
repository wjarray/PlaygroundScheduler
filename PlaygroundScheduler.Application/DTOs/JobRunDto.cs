namespace PlaygroundScheduler.Application.DTOs;

public sealed record JobRunDto(
    Guid Id,
    string Name,
    string CommandLine,
    string Type,
    int RetryCount,
    bool IsEnabled);