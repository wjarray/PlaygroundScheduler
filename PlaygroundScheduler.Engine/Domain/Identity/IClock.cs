namespace PlaygroundScheduler.Engine.Domain.Identity;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; set; }
    
}

