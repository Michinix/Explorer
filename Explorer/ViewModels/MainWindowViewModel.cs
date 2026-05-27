using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Explorer.Models;
using Explorer.Services;

namespace Explorer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly HomeViewModel _homeViewModel;

    [ObservableProperty] 
    private ObservableObject _currentPage = new SplashViewModel();
    
    [RelayCommand]
    private void GoHome() => CurrentPage = _homeViewModel;

    public MainWindowViewModel(HomeViewModel homeViewModel)
    {
        _homeViewModel = homeViewModel;

        Task.Run(async () =>
        {
            var list = FileSystemService.ListEntries(FileSystem.HomeDirectory);
            
            await Task.WhenAll(list, Task.Delay(2000));
            
            Dispatcher.UIThread.Post(() => GoHomeCommand.Execute(null));
        });
    }
}