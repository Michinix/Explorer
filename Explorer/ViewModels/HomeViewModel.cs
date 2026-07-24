using Explorer.Services;

namespace Explorer.ViewModels;

public class HomeViewModel : ViewModelBase
{
    public HomeViewModel(NavigationService navigationService, ClipboardService clipboardService)
    {
        DataGrid = new DataGridViewModel(navigationService);
        FileOperations = new FileOperationsViewModel(navigationService, clipboardService, DataGrid);
        DataGrid.FileOps = FileOperations;
        NavBar = new NavBarViewModel(navigationService, DataGrid, FileOperations);
        AsideLeft = new AsideLeftViewModel(navigationService);
        HomeContent = new HomeContentViewModel(navigationService);
    }

    public AsideLeftViewModel AsideLeft { get; }
    public DataGridViewModel DataGrid { get; }
    private FileOperationsViewModel FileOperations { get; }
    public NavBarViewModel NavBar { get; }
    public HomeContentViewModel HomeContent { get; }
}