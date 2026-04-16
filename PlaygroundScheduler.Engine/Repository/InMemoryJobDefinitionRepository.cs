using PlaygroundScheduler.Engine.Domain.Identity;

namespace PlaygroundScheduler.Engine.Repository;

public class InMemoryJobDefinitionRepository : IJobDefinitionRepository
{
    private readonly List<JobDefinition> _jobDefinitions = new List<JobDefinition>();

    public InMemoryJobDefinitionRepository()
    {
        
    }
    
    public InMemoryJobDefinitionRepository(IEnumerable<JobDefinition>? seed = null)
    {
        _jobDefinitions = seed?.ToList() ?? new List<JobDefinition>();
    }
    public Task<JobDefinition?> GetByIdAsync(JobDefinitionId definitionId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        JobDefinition? result = _jobDefinitions.FirstOrDefault(x => x.DefinitionId == definitionId);
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<JobDefinition>> ListAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        IReadOnlyList<JobDefinition> result = _jobDefinitions.AsReadOnly();
        return Task.FromResult(result);
    }
    
    public Task UpdateAsync(JobDefinition job, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var index = _jobDefinitions.FindIndex(x => x.DefinitionId == job.DefinitionId);
        if (index >= 0)
            _jobDefinitions[index] = job;
        else
            _jobDefinitions.Add(job);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(JobDefinitionId definitionId, CancellationToken ct = default)
    {
        var existing = _jobDefinitions.FirstOrDefault(x => x.DefinitionId == definitionId);
        if (existing is null)
            return Task.CompletedTask;

        _jobDefinitions.Remove(existing);
        return Task.CompletedTask;
    }
}