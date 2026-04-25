using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PlaygroundScheduler.Avalonia.App.ViewModels.DTO;

public sealed partial class JobEditorViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid? id;

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string commandLine = "";

    [ObservableProperty]
    private string selectedType = "Shell";

    [ObservableProperty]
    private int retryCount;

    [ObservableProperty]
    private bool isEnabled = true;

    public bool IsNew => Id is null;
}