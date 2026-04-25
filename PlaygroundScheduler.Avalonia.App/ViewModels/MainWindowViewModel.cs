using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    
    public async Task LoadAsync(CancellationToken ct = default)
    {
        if (CurrentViewModel is ILoadableViewModel loadable)
            await loadable.LoadAsync(ct);
    }

    [RelayCommand]
    private async Task ShowDashboard()
    {
        CurrentViewModel = DashboardVm;
        await LoadCurrentPageAsync();
    }

    [RelayCommand]
    private async Task ShowJobs()
    {
        CurrentViewModel = JobsVm;
        await LoadCurrentPageAsync();
        
    }

    [RelayCommand]
    private async Task ShowSettings()
    {
        CurrentViewModel = SettingsVm;
        await LoadCurrentPageAsync();
        
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
    
    private async Task LoadCurrentPageAsync()
    {
        if (CurrentViewModel is ILoadableViewModel loadable)
            await loadable.LoadAsync();
    }
}