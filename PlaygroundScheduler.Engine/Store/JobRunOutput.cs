using PlaygroundScheduler.Engine.Domain.Identity;

namespace PlaygroundScheduler.Engine.Store;

public sealed class JobRunOutput
{
    public JobRunId RunId { get; set; }


    public string StdOutput { get; set; }
    
    public string StdErr { get; set; }
    public JobRunOutput(JobRunId runId, string stdOutput, string stdErr)
    {
        RunId = runId;
        StdOutput = stdOutput;
        StdErr = stdErr;
    }
}