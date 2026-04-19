using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlaygroundScheduler.Avalonia.App.Views.Components;

namespace PlaygroundScheduler.Avalonia.App.ViewModels;

public partial class JobsViewModel : ViewModelBase
{
    public ObservableCollection<JobItemViewModel> Items { get; } = [];

    [ObservableProperty] private JobItemViewModel? selectedItem;
    [ObservableProperty] private string? searchText;
    [ObservableProperty] private JobEditorViewModel? editor = new();

    public List<string> AvailableTypes { get; } = ["Shell", "Python", "Dotnet"];

    partial void OnSelectedItemChanged(JobItemViewModel? value)
    {
        if (value is null)
        {
            Editor = new JobEditorViewModel();
            return;
        }

        Editor = new JobEditorViewModel
        {
            Name = value.Name,
            CommandLine = value.CommandLine,
            RetryCount = value.RetryCount,
            IsEnabled = value.IsEnabled,
            SelectedType = value.Type
        };
    }

    public JobsViewModel()
    {
        Items.Add(new JobItemViewModel
        {
            Name = "Night Backup",
            Status = "Idle",
            CommandLine = "backup.sh",
            RetryCount = 3,
            IsEnabled = true,
            Type = "Shell"
        });
        
        Items.Add(new JobItemViewModel
        {
            Name = "Generate Reports",
            Status = "Running",
            CommandLine = "report.py",
            RetryCount = 1,
            IsEnabled = true,
            Type = "Python"
        });
    }

    [RelayCommand]
    private void Create()
    {
        SelectedItem = null;
        Editor = new JobEditorViewModel();
    }

    [RelayCommand]
    private void Refresh()
    {
        //
    }
    
    [RelayCommand]
    private void Save()
    {
        //
    }
    
    [RelayCommand]
    private void Delete()
    {
        //
    }
}