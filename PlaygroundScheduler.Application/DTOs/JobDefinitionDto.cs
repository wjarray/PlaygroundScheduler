namespace PlaygroundScheduler.Application.DTOs;

public sealed record JobDefinitionDto(
    Guid Id,
    string Name,
    string CommandLine,
    string Type,
    int RetryCount,
    bool IsEnabled);