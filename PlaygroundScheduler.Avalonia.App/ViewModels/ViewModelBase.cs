using CommunityToolkit.Mvvm.ComponentModel;

namespace PlaygroundScheduler.Avalonia.App.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    public virtual string TitleSegment => "";
    
    public virtual ViewModelBase? ActiveChild => null;
}

