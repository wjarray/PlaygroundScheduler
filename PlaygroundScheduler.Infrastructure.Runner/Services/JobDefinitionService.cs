using PlaygroundScheduler.Application.DTOs;
using PlaygroundScheduler.Application.Repository;
using PlaygroundScheduler.Application.Services;
using PlaygroundScheduler.Domain.Identity;

namespace PlaygroundScheduler.Infrastructure.Runner.Services;

public sealed class JobDefinitionService : IJobDefinitionService
{
    private readonly IJobDefinitionRepository _repository;

    public JobDefinitionService(IJobDefinitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<JobDefinitionDto>> GetAllAsync(
        CancellationToken ct = default)
    {
        var definitions = await _repository.ListAsync(ct);

        return definitions
            .Select(MapToDto)
            .ToList();
    }
    
    public async Task<JobDefinitionDto> AddAsync(
        string name,
        string commandLine,
        string type,
        int retryCount,
        bool isEnabled,
        CancellationToken ct = default)
    {
        Validate(name, commandLine, retryCount);

        var definition = new JobDefinition(
            JobDefinitionId.New(),
            name,
            commandLine,
            retryCount);

        await _repository.CreateAsync(definition, ct);

        return MapToDto(definition, type, retryCount, isEnabled);
    }

    public async Task<JobDefinitionDto> UpdateAsync(
        Guid id,
        string name,
        string commandLine,
        string type,
        int retryCount,
        bool isEnabled,
        CancellationToken ct = default)
    {
        Validate(name, commandLine, retryCount);

        var jobDefinitionId = new JobDefinitionId(id);

        var existing = await _repository.GetByIdAsync(jobDefinitionId, ct);

        if (existing is null)
            throw new InvalidOperationException($"Job definition '{id}' was not found.");

        var updated = new JobDefinition(
            jobDefinitionId,
            name,
            commandLine, 
            0);

        await _repository.UpdateAsync(updated, ct);

        return MapToDto(updated, type, retryCount, isEnabled);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var jobDefinitionId = new JobDefinitionId(id);

        var existing = await _repository.GetByIdAsync(jobDefinitionId, ct);

        if (existing is null)
            throw new InvalidOperationException($"Job definition '{id}' was not found.");

        await _repository.DeleteAsync(jobDefinitionId, ct);
    }

    private static void Validate(
        string name,
        string commandLine,
        int retryCount)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Job name is required.");

        if (string.IsNullOrWhiteSpace(commandLine))
            throw new InvalidOperationException("Command line is required.");

        if (retryCount < 0)
            throw new InvalidOperationException("Retry count cannot be negative.");
    }

    private static JobDefinitionDto MapToDto(JobDefinition definition)
    {
        return new JobDefinitionDto(
            Id: definition.DefinitionId.Value,
            Name: definition.Name,
            CommandLine: definition.CommandLine,
            Type: "Shell",
            RetryCount: 0,
            IsEnabled: true);
    }

    private static JobDefinitionDto MapToDto(
        JobDefinition definition,
        string type,
        int retryCount,
        bool isEnabled)
    {
        return new JobDefinitionDto(
            Id: definition.DefinitionId.Value,
            Name: definition.Name,
            CommandLine: definition.CommandLine,
            Type: type,
            RetryCount: retryCount,
            IsEnabled: isEnabled);
    }
}