using Explorer.Services;

namespace Explorer.ViewModels;

public class HomeViewModel(NavigationService navigation) : ViewModelBase
{
    public DataGridViewModel DataGrid { get; } = new(navigation);
    public NavBarViewModel NavBar { get; } = new(navigation);
}