using Explorer.Services;

namespace Explorer.ViewModels;

public class HomeViewModel : ViewModelBase
{
    public HomeViewModel(FileSystemService fileSystemService, NavigationService navigationService)
    {
        DataGrid = new DataGridViewModel(navigationService, fileSystemService);
        NavBar = new NavBarViewModel(navigationService, DataGrid);
        AsideLeft = new AsideLeftViewModel(navigationService, fileSystemService);
    }

    public AsideLeftViewModel AsideLeft { get; }
    public DataGridViewModel DataGrid { get; }
    public NavBarViewModel NavBar { get; }
}