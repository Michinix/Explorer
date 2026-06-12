using Explorer.Services;

namespace Explorer.ViewModels;

public class NavBarViewModel(NavigationService navigation) : ViewModelBase
{
    public NavigationService Navigation { get; } = navigation;
}