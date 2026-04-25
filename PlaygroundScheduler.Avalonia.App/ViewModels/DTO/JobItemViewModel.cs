using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PlaygroundScheduler.Avalonia.App.ViewModels.DTO;

public sealed partial class JobItemViewModel : ObservableObject
{
    public Guid Id { get; init; }

    public string Name { get; init; } = "";
    public string CommandLine { get; init; } = "";
    public string Type { get; init; } = "Shell";
    public int RetryCount { get; init; }
    public bool IsEnabled { get; init; }

    public string Status { get; init; } = "Pending";
}