using Explorer.Services;

namespace Explorer.ViewModels;

public class HomeViewModel(NavigationService navigation, DataGridViewModel dataGrid) : ViewModelBase
{
    public AsideLeftViewModel AsideLeft { get; } = new(navigation);
    public DataGridViewModel DataGrid { get; } = dataGrid;
    public NavBarViewModel NavBar { get; } = new(navigation, dataGrid);
}