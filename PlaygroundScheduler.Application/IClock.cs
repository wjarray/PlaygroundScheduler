namespace PlaygroundScheduler.Application;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; set; }
    
}

