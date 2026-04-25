using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using PlaygroundScheduler.Application;
using PlaygroundScheduler.Application.Repository;
using PlaygroundScheduler.Application.Runner;
using PlaygroundScheduler.Application.Services;
using PlaygroundScheduler.Avalonia.App.ViewModels;
using PlaygroundScheduler.Avalonia.App.Views;
using PlaygroundScheduler.Infrastructure.Runner.Db;
using PlaygroundScheduler.Infrastructure.Runner.Db.Connection;
using PlaygroundScheduler.Infrastructure.Runner.Repository;
using PlaygroundScheduler.Infrastructure.Runner.Runner;
using PlaygroundScheduler.Infrastructure.Runner.Services;

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
      
        // Setting DB 
        services.AddSingleton<IDatabasePathProvider, DesktopDatabasePathProvider>();
        services.AddSingleton<IDatabaseInitializer,SqliteDatabaseInitializer>();
        services.AddSingleton<ISqliteConnectionFactory, SqliteConnectionFactory>();
        
        // Persistence
        services.AddSingleton<IJobDefinitionRepository, SqliteJobDefinitionRepository>();
        services.AddSingleton<IJobRunRepository, SqliteJobRunRepository>();

        //Setting runner
        services.AddSingleton<IClock, FakeClock>();
        services.AddSingleton<ILocalJobRunner, LocalJobRunner>();
        
        //Setting View Models
        
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<JobsViewModel>();
        services.AddSingleton<SettingsViewModel>();

        // Application services
        services.AddSingleton<IJobDefinitionService, JobDefinitionService>();

      
        
        //Setting View
        services.AddTransient<MainWindow>();
        
     
        var serviceProvider = services.BuildServiceProvider();
        
        
        serviceProvider
            .GetRequiredService<IDatabaseInitializer>()
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