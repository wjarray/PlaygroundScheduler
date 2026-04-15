using PlaygroundScheduler.Engine.Domain.Identity;

namespace PlaygroundScheduler.Engine.Repository;

public class JobRunRepository : IJobRunRepository
{
    private readonly List<JobRun> _jobRuns = new List<JobRun>();
    
    public JobRunRepository()
    {
        
    }
    public JobRunRepository(IEnumerable<JobRun>? seed = null)
    {
        _jobRuns = seed?.ToList() ?? new List<JobRun>();
    }
    public Task<JobRun?> GetByIdAsync(JobRunId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        JobRun? result = _jobRuns.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<JobRun>> ListByJobIdAsync(JobDefinitionId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        IReadOnlyList<JobRun> result = _jobRuns.Where(x => x.JobDefinitionId == id).ToList();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<JobRun>> ListRecentAsync(int count, CancellationToken ct = default)
    {
       ct.ThrowIfCancellationRequested();
       IReadOnlyList<JobRun> result = _jobRuns.OrderByDescending(x => x.CreatedAt).Take(count).ToList();
       return Task.FromResult(result);
    }

    public Task CreateAsync(JobRun run, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(run);

        if (_jobRuns.Any(x => x.Id == run.Id))
            throw new InvalidOperationException($"Run '{run.Id}' already exists.");

        _jobRuns.Add(run);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(JobRun run, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var index = _jobRuns.FindIndex(x => x.Id == run.Id);
        if (index >= 0)
            _jobRuns[index] = run;
        else
            _jobRuns.Add(run);
        return Task.CompletedTask;
    }
}