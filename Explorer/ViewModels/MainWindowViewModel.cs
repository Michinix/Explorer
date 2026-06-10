using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Explorer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly HomeViewModel _homeViewModel;

    [ObservableProperty] private ObservableObject _currentPage = new SplashViewModel();

    [RelayCommand]
    private void GoHome() => CurrentPage = _homeViewModel;

    public MainWindowViewModel(HomeViewModel homeViewModel)
    {
        _homeViewModel = homeViewModel;

        Task.Run(async () =>
        {
            await Task.WhenAll(
                _homeViewModel.DataGrid.LoadEntriesAsync(),
                Task.Delay(2000)
            );

            await Dispatcher.UIThread.InvokeAsync(
                () => GoHomeCommand.Execute(null),
                DispatcherPriority.Background
            );
        });
    }
}