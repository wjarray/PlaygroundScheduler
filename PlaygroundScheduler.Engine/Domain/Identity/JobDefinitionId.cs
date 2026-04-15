namespace PlaygroundScheduler.Engine.Domain.Identity;

public readonly record struct JobDefinitionId(Guid Value)
{
    public static JobDefinitionId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}