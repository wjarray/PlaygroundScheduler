using CommunityToolkit.Mvvm.ComponentModel;

namespace PlaygroundScheduler.Avalonia.App.ViewModels;

public partial class JobEditorViewModel : ViewModelBase
{
    [ObservableProperty] private string name = "";
    [ObservableProperty] private string commandLine = "";
    [ObservableProperty] private int retryCount;
    [ObservableProperty] private bool isEnabled;
    [ObservableProperty] private string? selectedType;
}