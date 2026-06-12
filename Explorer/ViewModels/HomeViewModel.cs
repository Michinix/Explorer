using Explorer.Services;

namespace Explorer.ViewModels;

public partial class HomeViewModel(NavigationService navigation) : ViewModelBase
{
    public DataGridViewModel DataGrid { get; } = new(navigation);
    public NavBarViewModel NavBar { get; } = new(navigation);
}