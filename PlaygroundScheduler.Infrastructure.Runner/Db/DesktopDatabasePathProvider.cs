namespace PlaygroundScheduler.Infrastructure.Runner.Db.Connection;

public sealed class DesktopDatabasePathProvider : IDatabasePathProvider
{
    public string DatabasePath
    {
        get
        {
            var appData = Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData);

            return Path.Combine(
                appData,
                "PlaygroundScheduler",
                "scheduler.sqlite");
        }
    }

}