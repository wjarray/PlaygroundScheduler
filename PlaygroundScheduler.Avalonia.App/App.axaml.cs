using System;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PlaygroundScheduler.Avalonia.App.Views;

namespace PlaygroundScheduler.Avalonia.App;

public partial class App : global::Avalonia.Application
{
    public static IServiceProvider ServiceProvider { get; set; } = null!;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        }
        base.OnFrameworkInitializationCompleted();
    }
}