using Explorer.Services;

namespace Explorer.ViewModels;

public class HomeViewModel : ViewModelBase
{
    public HomeViewModel(NavigationService navigationService, ClipboardService clipboardService)
    {
        DataGrid = new DataGridViewModel(navigationService, clipboardService);
        NavBar = new NavBarViewModel(navigationService, DataGrid);
        AsideLeft = new AsideLeftViewModel(navigationService);
    }

    public AsideLeftViewModel AsideLeft { get; }
    public DataGridViewModel DataGrid { get; }
    public NavBarViewModel NavBar { get; }
}