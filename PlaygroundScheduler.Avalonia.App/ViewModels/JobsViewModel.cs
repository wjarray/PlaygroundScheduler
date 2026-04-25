using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlaygroundScheduler.Application.DTOs;
using PlaygroundScheduler.Application.Services;
using PlaygroundScheduler.Avalonia.App.ViewModels.DTO;

namespace PlaygroundScheduler.Avalonia.App.ViewModels;

public partial class JobsViewModel : ViewModelBase, ILoadableViewModel
{
    private readonly IJobDefinitionService _jobDefinitionService;
    public override string TitleSegment { get; } = "Jobs";
    public List<string> AvailableTypes { get; } = ["Shell", "Python", "Dotnet"];

    public ObservableCollection<JobItemViewModel> Items { get; } = [];

    [ObservableProperty] private JobItemViewModel? selectedItem;
    [ObservableProperty] private string? searchText;
    [ObservableProperty] private JobEditorViewModel? editor = new();
   
    [ObservableProperty] private bool isBusy;

    [ObservableProperty] private string? errorMessage;
    
    public JobsViewModel(IJobDefinitionService jobDefinitionService)
    {
        _jobDefinitionService = jobDefinitionService;
    }
    
    protected JobsViewModel()
    {
        _jobDefinitionService = null!;
    }
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
    
    private async Task LoadItemsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var definitions = await _jobDefinitionService.GetAllAsync();

            Items.Clear();

            foreach (var definition in definitions)
            {
                Items.Add(MapToItem(definition));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Create()
    {
        SelectedItem = null;
        Editor = new JobEditorViewModel
        {
            SelectedType = AvailableTypes.FirstOrDefault() ?? "Shell",
            IsEnabled = true
        };
        ErrorMessage = null;
    }
    
    public async Task LoadAsync(CancellationToken ct = default)
    {
        await LoadItemsAsync(ct);
    }
    
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadItemsAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await RunBusyAsync(async ct =>
        {
            if (string.IsNullOrWhiteSpace(Editor.Name))
                throw new InvalidOperationException("Job name is required.");

            if (string.IsNullOrWhiteSpace(Editor.CommandLine))
                throw new InvalidOperationException("Command line is required.");

            if (Editor.Id is null)
            {
                var created = await _jobDefinitionService.AddAsync(
                    Editor.Name,
                    Editor.CommandLine,
                    Editor.SelectedType,
                    Editor.RetryCount,
                    Editor.IsEnabled,
                    ct);

                var item = MapToItem(created);
                Items.Add(item);

                SelectedItem = item;
            }
            else
            {
                var updated = await _jobDefinitionService.UpdateAsync(
                    Editor.Id.Value,
                    Editor.Name,
                    Editor.CommandLine,
                    Editor.SelectedType,
                    Editor.RetryCount,
                    Editor.IsEnabled,
                    ct);

                ReplaceItem(MapToItem(updated));
            }
        });
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedItem is null)
            return;

        var itemToDelete = SelectedItem;

        await RunBusyAsync(async ct =>
        {
            await _jobDefinitionService.DeleteAsync(itemToDelete.Id, ct);

            Items.Remove(itemToDelete);

            SelectedItem = null;
            Editor = new JobEditorViewModel();
        });
    }

    private void ReplaceItem(JobItemViewModel updatedItem)
    {
        var index = Items
            .Select((item, index) => new { item, index })
            .FirstOrDefault(x => x.item.Id == updatedItem.Id)
            ?.index;

        if (index is null)
            return;

        Items[index.Value] = updatedItem;
        SelectedItem = updatedItem;
    }

    private static JobItemViewModel MapToItem(JobDefinitionDto definition)
    {
        return new JobItemViewModel
        {
            Id = definition.Id,
            Name = definition.Name,
            CommandLine = definition.CommandLine,
            Type = definition.Type,
            RetryCount = definition.RetryCount,
            IsEnabled = definition.IsEnabled,
            Status = "Pending"
        };
    }

    private async Task RunBusyAsync(Func<CancellationToken, Task> action)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            await action(CancellationToken.None);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    private async Task LoadItemsAsync(CancellationToken ct)
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var definitions = await _jobDefinitionService.GetAllAsync(ct);

            Items.Clear();

            foreach (var definition in definitions)
                Items.Add(MapToItem(definition));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
  
}