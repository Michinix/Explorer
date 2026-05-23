using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
            await Task.Delay(3000);
            GoHomeCommand.Execute(null);
        });
    }
}