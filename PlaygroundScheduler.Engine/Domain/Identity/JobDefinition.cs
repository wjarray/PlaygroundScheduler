using System.ComponentModel;

namespace PlaygroundScheduler.Engine.Domain.Identity;

public sealed class JobDefinition
{
    public JobDefinitionId DefinitionId { get; }
    public string Name { get; }
    public string CommandLine { get; }
    public int MaxRetryCount { get; }

    public JobDefinition(JobDefinitionId definitionId, string name, string commandLine, int maxRetryCount)
    {
        if (definitionId.Value == Guid.Empty)
            throw new InvalidEnumArgumentException($"{nameof(definitionId)} cannot be empty");
        if (string.IsNullOrEmpty(name))
            throw new InvalidEnumArgumentException($"{nameof(name)} cannot be null");
        if (string.IsNullOrEmpty(commandLine))
            throw new InvalidEnumArgumentException($"{nameof(commandLine)} cannot be null");
        if (maxRetryCount < 0)
            throw new InvalidEnumArgumentException($"{nameof(maxRetryCount)} cannot be negative");

        DefinitionId = definitionId;
        CommandLine = commandLine;
        Name = name;
        MaxRetryCount = maxRetryCount;
    }

    public JobDefinition Rename(string pNewName)
    {
        if (string.IsNullOrEmpty(pNewName))
            throw new InvalidEnumArgumentException($"{nameof(pNewName)} cannot be null");

        return new JobDefinition(DefinitionId, pNewName, CommandLine, MaxRetryCount);
    }


    public JobDefinition ChangeCommandLine(string pCommandline)
    {
        if (string.IsNullOrEmpty(pCommandline))
            throw new InvalidEnumArgumentException($"{nameof(pCommandline)} cannot be null");

        return new JobDefinition(DefinitionId, Name, pCommandline, MaxRetryCount);
    }

    public JobDefinition ChangeMaxRetryCount(int pMaxRetryCount)
    {
        if (pMaxRetryCount < 0)
            throw new InvalidEnumArgumentException($"{nameof(pMaxRetryCount)} cannot be negative");

        return new JobDefinition(DefinitionId, Name, CommandLine, pMaxRetryCount);
    }
}