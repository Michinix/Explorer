using Explorer.Services;

namespace Explorer.ViewModels;

public class HomeViewModel : ViewModelBase
{
	public HomeViewModel(NavigationService navigationService, ClipboardService clipboardService,
		SettingsService settingsService, OcrService ocrService)
	{
		FileBrowser = new FileBrowserViewModel(navigationService, settingsService, ocrService);
		FileOperations = new FileOperationsViewModel(navigationService, clipboardService, FileBrowser, settingsService);
		FileBrowser.FileOps = FileOperations;
		NavBar = new NavBarViewModel(navigationService, FileBrowser, FileOperations);
		AsideLeft = new AsideLeftViewModel(navigationService, settingsService);
		HomeContent = new HomeContentViewModel(navigationService, settingsService);
	}

	public AsideLeftViewModel AsideLeft { get; }
	public FileBrowserViewModel FileBrowser { get; }
	private FileOperationsViewModel FileOperations { get; }
	public NavBarViewModel NavBar { get; }
	public HomeContentViewModel HomeContent { get; }
}