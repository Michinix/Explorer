using Explorer.Services;

namespace Explorer.ViewModels;

public class HomeViewModel : ViewModelBase
{
    public HomeViewModel(NavigationService navigationService)
    {
        DataGrid = new DataGridViewModel(navigationService);
        NavBar = new NavBarViewModel(navigationService, DataGrid);
        AsideLeft = new AsideLeftViewModel(navigationService);
    }

    public AsideLeftViewModel AsideLeft { get; }
    public DataGridViewModel DataGrid { get; }
    public NavBarViewModel NavBar { get; }
}