using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlaygroundScheduler.Avalonia.App.Views.Components;

namespace PlaygroundScheduler.Avalonia.App.ViewModels;

public partial class JobsViewModel : ViewModelBase
{
    public override string TitleSegment { get; } = "Jobs";

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
        var item = new JobItemViewModel
        {
            Name = Editor.Name,
            Status = "Pending",
            CommandLine = Editor.CommandLine,
            RetryCount = Editor.RetryCount,
            IsEnabled = true,
            Type = Editor.SelectedType
        };
        
        Items.Add(item);
    }
    
    [RelayCommand]
    private void Delete()
    {
        Items.Remove(SelectedItem);
    }
}