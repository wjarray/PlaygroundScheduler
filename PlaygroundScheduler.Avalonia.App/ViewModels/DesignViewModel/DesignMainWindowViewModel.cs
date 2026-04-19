using PlaygroundScheduler.Avalonia.App.Views;

namespace PlaygroundScheduler.Avalonia.App.ViewModels.DesignViewModel;

public class DesignMainWindowViewModel : MainWindowViewModel
{
    public  DashboardViewModel DashboardVm { get; }
    public JobsViewModel JobsVm { get; }
    public SettingsViewModel SettingsVm { get; }


    public DesignMainWindowViewModel()
    {
        DashboardVm = new DashboardViewModel();
        JobsVm = new DesignJobsViewModel();
        SettingsVm = new SettingsViewModel();

        CurrentViewModel = JobsVm;
    }
    
}