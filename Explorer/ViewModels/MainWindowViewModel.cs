using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Explorer.Services;

namespace Explorer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly HomeViewModel _homeViewModel;

    [ObservableProperty] 
    private ObservableObject _currentPage = new SplashViewModel();
    
    [RelayCommand]
    private void GoHome() => CurrentPage = _homeViewModel;

    public MainWindowViewModel(HomeViewModel homeViewModel, BrowserService browserService)
    {
        _homeViewModel = homeViewModel;

        Task.Run(async () =>
        {
            var directoriesTask = browserService.ListDirectories();
            var filesTask = browserService.ListFiles();
    
            await Task.WhenAll(directoriesTask, filesTask, Task.Delay(2000));
    
            _homeViewModel.Directories = directoriesTask.Result;
            _homeViewModel.Files = filesTask.Result;
    
            Dispatcher.UIThread.Post(() => GoHomeCommand.Execute(null));
        });
    }
}