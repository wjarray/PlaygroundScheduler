using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PlaygroundScheduler.Avalonia.App.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private ViewModelBase? currentViewModel;

    public DashboardViewModel DashboardVm { get; }
    public JobsViewModel JobsVm { get; }
    public SettingsViewModel SettingsVm { get; }

    public MainViewModel()
    {
        DashboardVm = new DashboardViewModel();
        JobsVm = new JobsViewModel();
        SettingsVm = new SettingsViewModel();

        CurrentViewModel = DashboardVm;
    }

    [RelayCommand]
    private void ShowDashboard() => CurrentViewModel = DashboardVm;

    [RelayCommand]
    private void ShowJobs() => CurrentViewModel = JobsVm;

    [RelayCommand]
    private void ShowSettings() => CurrentViewModel = SettingsVm;
}