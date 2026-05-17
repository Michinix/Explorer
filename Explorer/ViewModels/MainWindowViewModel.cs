using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Explorer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] 
    private ObservableObject _currentPage = new SplashViewModel();
    
    [RelayCommand]
    private void GoHome() => CurrentPage = new HomeViewModel();

    public MainWindowViewModel()
    {
        Task.Run(() =>
        {
            Task.Delay(3000).Wait();
            GoHomeCommand.Execute(null);
        });
    }
}