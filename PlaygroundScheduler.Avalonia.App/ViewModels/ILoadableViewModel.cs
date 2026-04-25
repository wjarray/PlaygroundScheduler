using System.Threading;
using System.Threading.Tasks;

namespace PlaygroundScheduler.Avalonia.App.ViewModels;

public interface ILoadableViewModel
{
    Task LoadAsync(CancellationToken ct = default);
}