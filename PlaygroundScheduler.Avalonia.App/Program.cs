using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using PlaygroundScheduler.Application;
using PlaygroundScheduler.Application.Runner;
using PlaygroundScheduler.Application.Services;
using PlaygroundScheduler.Avalonia.App.Views;
using PlaygroundScheduler.Infrastructure.Runner.Db;
using PlaygroundScheduler.Infrastructure.Runner.Db.Connection;
using PlaygroundScheduler.Infrastructure.Runner.Runner;

namespace PlaygroundScheduler.Avalonia.App;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    public static void Main(string[] args)
    {
        SQLitePCL.Batteries.Init();
        var services = new ServiceCollection();

        services.AddSingleton<IClock, FakeClock>();
        services.AddSingleton<ILocalJobRunner, LocalJobRunner>();
        services.AddTransient<MainWindow>();
        
        services.AddSingleton<IDatabasePathProvider, DesktopDatabasePathProvider>();
        services.AddSingleton<SqliteDatabaseInitializer>();
        services.AddScoped<SqliteConnectionFactory>();
        var serviceProvider = services.BuildServiceProvider();
        
        serviceProvider
            .GetRequiredService<SqliteDatabaseInitializer>()
            .EnsureCreatedAsync()
            .GetAwaiter()
            .GetResult();
        
        
        App.ServiceProvider = serviceProvider;
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}