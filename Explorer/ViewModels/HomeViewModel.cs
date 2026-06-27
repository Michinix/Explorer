using Explorer.Services;

namespace Explorer.ViewModels;

public class HomeViewModel(NavigationService navigation, FileSystemService fileSystemService) : ViewModelBase
{
    public NavBarViewModel NavBar { get; } = new(navigation);
    public AsideLeftViewModel AsideLeft { get; } = new(navigation);
    public DataGridViewModel DataGrid { get; } = new(navigation, fileSystemService);
}