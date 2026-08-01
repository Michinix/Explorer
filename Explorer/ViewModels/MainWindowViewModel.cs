using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Explorer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
	[ObservableProperty] private HomeViewModel? _homeContent;
	[ObservableProperty] private bool _isSplashVisible = true;

	public MainWindowViewModel(HomeViewModel homeViewModel)
	{
		HomeContent = homeViewModel;

		Task.Run(async () =>
		{
			await HomeContent.FileBrowser.LoadEntriesAsync();

			await Task.Delay(2500);

			IsSplashVisible = false;
		});
	}
}