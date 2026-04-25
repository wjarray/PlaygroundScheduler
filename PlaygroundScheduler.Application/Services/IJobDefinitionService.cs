using PlaygroundScheduler.Application.DTOs;
using PlaygroundScheduler.Domain.Identity;

namespace PlaygroundScheduler.Application.Services;

public interface IJobDefinitionService
{
    Task<IReadOnlyList<JobDefinitionDto>> GetAllAsync(CancellationToken ct = default);

    Task<JobDefinitionDto> AddAsync(
        string name,
        string commandLine,
        string type,
        int retryCount,
        bool isEnabled,
        CancellationToken ct = default);
    

    Task<JobDefinitionDto> UpdateAsync(
        Guid id,
        string name,
        string commandLine,
        string type,
        int retryCount,
        bool isEnabled,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
