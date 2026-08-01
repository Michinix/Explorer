using Explorer.Services;

namespace Explorer.ViewModels;

public class NavBarViewModel(
	NavigationService navigation,
	FileBrowserViewModel fileBrowser,
	FileOperationsViewModel fileOperations) : ViewModelBase
{
	public NavigationService Navigation { get; } = navigation;
	public FileBrowserViewModel FileBrowser { get; } = fileBrowser;
	public FileOperationsViewModel FileOperations { get; } = fileOperations;
}