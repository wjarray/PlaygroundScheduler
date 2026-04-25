using Avalonia.Controls;
using PlaygroundScheduler.Avalonia.App.ViewModels;

namespace PlaygroundScheduler.Avalonia.App.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}