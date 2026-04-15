using PlaygroundScheduler.Engine.Domain.Identity;

namespace PlaygroundScheduler.Engine.Repository;

public interface IJobDefinitionRepository
{
    Task<JobDefinition?> GetByIdAsync(JobDefinitionId id, CancellationToken ct = default);
    Task<IReadOnlyList<JobDefinition>> ListAsync(CancellationToken ct = default);
    Task UpdateAsync(JobDefinition job, CancellationToken ct = default);
    Task DeleteAsync(JobDefinitionId definitionId, CancellationToken ct = default);
}