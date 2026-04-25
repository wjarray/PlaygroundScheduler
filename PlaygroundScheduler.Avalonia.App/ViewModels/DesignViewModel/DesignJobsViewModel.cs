using PlaygroundScheduler.Avalonia.App.ViewModels.DTO;

namespace PlaygroundScheduler.Avalonia.App.ViewModels.DesignViewModel;

public class DesignJobsViewModel : JobsViewModel
{
    public DesignJobsViewModel()
    {
        Items.Clear();

        Items.Add(new JobItemViewModel { Name = "Mock Job A", Status = "Pending" });
        Items.Add(new JobItemViewModel { Name = "Mock Job B", Status = "Running" });
        Items.Add(new JobItemViewModel { Name = "Mock Job C", Status = "Succeeded" });
    }
}