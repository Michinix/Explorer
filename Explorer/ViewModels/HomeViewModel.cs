namespace Explorer.ViewModels;

public class HomeViewModel(
    AsideLeftViewModel asideLeft,
    DataGridViewModel dataGrid,
    NavBarViewModel navBar) : ViewModelBase
{
    public AsideLeftViewModel AsideLeft { get; } = asideLeft;
    public DataGridViewModel DataGrid { get; } = dataGrid;
    public NavBarViewModel NavBar { get; } = navBar;
}