using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PlaygroundScheduler.Avalonia.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private ViewModelBase? currentViewModel;

    [ObservableProperty] private string selectedPageTitle;

    public string AppTitle => "Scheduler";
    
    public DashboardViewModel DashboardVm { get; }
    public JobsViewModel JobsVm { get; }
    public SettingsViewModel SettingsVm { get; }

    public MainWindowViewModel(DashboardViewModel dashboardVm, JobsViewModel jobViewModel, SettingsViewModel settingsViewModel )
    {
        DashboardVm = dashboardVm;
        JobsVm = jobViewModel;
        SettingsVm = settingsViewModel;
        
        CurrentViewModel = JobsVm;
        
    }

    protected MainWindowViewModel()
    {
    }

    [RelayCommand]
    private void ShowDashboard()
    {
        CurrentViewModel = DashboardVm;
    }

    [RelayCommand]
    private void ShowJobs()
    {
        CurrentViewModel = JobsVm;
    }


    [RelayCommand]
    private void ShowSettings()
    {
        CurrentViewModel = SettingsVm;
    }

    partial void OnCurrentViewModelChanged(ViewModelBase? value)
    {
        RefreshTitle();
    }

    private void RefreshTitle()
    {
        var segments = GetSegments(CurrentViewModel);
        SelectedPageTitle = string.Join(" ", new[] { AppTitle }.Concat(segments));
    }

    private static IEnumerable<string> GetSegments(ViewModelBase? vm)
    {
        var current = vm;
        while (current is not null)
        {
            if (!string.IsNullOrWhiteSpace(current.TitleSegment))
                yield return current.TitleSegment;

            current = current.ActiveChild;
        }
    }
}