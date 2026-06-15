using Explorer.Services;

namespace Explorer.ViewModels;

public partial class NavBarViewModel(NavigationService navigation) : ViewModelBase
{
    public NavigationService Navigation { get; } = navigation;
}