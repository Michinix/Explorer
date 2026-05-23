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

    public MainWindowViewModel(HomeViewModel homeViewModel, Browser browser)
    {
        _homeViewModel = homeViewModel;

        Task.Run(async () =>
        {
            var directoriesTask = BrowserService.ListDirectories(browser.HomeDirectory);
            var filesTask = BrowserService.ListFiles(browser.HomeDirectory);
    
            await Task.WhenAll(directoriesTask, filesTask, Task.Delay(2000));
    
            _homeViewModel.Directories = directoriesTask.Result;
            _homeViewModel.Files = filesTask.Result;
    
            Dispatcher.UIThread.Post(() => GoHomeCommand.Execute(null));
        });
    }
}