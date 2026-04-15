namespace PlaygroundScheduler.Engine.Domain.Identity;

public readonly record struct JobRunId(Guid Value)
{
    public static JobRunId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}