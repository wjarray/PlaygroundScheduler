namespace PlaygroundScheduler.Infrastructure.Runner.Db.Connection;

public interface IDatabaseInitializer
{
    Task EnsureCreatedAsync(CancellationToken ct = default);
}