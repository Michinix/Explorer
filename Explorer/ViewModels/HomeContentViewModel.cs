using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Explorer.Models;
using Explorer.Services;

namespace Explorer.ViewModels;

public partial class HomeContentViewModel : ViewModelBase
{
	private readonly NavigationService _navigation;
	private readonly SettingsService _settings;

	[ObservableProperty] [NotifyPropertyChangedFor(nameof(HasCloudStorage))]
	private IReadOnlyList<DriveItem> _cloudStorage = [];

	[ObservableProperty] private IReadOnlyList<DriveItem> _drives = [];

	[ObservableProperty] private bool _isActive;

	public HomeContentViewModel(NavigationService navigation, SettingsService settings)
	{
		_navigation = navigation;
		_settings = settings;

		RecentFiles.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasRecentFiles));

		_ = LoadDrivesAsync();
		_ = LoadCloudStorageAsync();
		IsActive = string.Equals(navigation.CurrentPath, NavigationService.HomePath,
			StringComparison.OrdinalIgnoreCase);

		WeakReferenceMessenger.Default.Register<CurrentPathChangedMessage>(this, (_, message) =>
			IsActive = string.Equals(message.NewPath, NavigationService.HomePath, StringComparison.OrdinalIgnoreCase));
	}

	public bool HasCloudStorage => CloudStorage.Count > 0;

	public ObservableCollection<PinnedItem> PinnedItems => _settings.PinnedItems;
	public ObservableCollection<RecentFile> RecentFiles => _settings.RecentFiles;

	public bool HasRecentFiles => RecentFiles.Count > 0;

	private async Task LoadDrivesAsync()
	{
		Drives = await FileSystemService.GetDrivesAsync();
	}

	private async Task LoadCloudStorageAsync()
	{
		CloudStorage = await FileSystemService.GetCloudStorageAsync();
	}

	[RelayCommand]
	private void OpenQuickAccess(string path)
	{
		_navigation.NavigateTo(path);
	}

	[RelayCommand]
	private void RemovePinned(string path)
	{
		_settings.RemovePinned(path);
	}

	[RelayCommand]
	private async Task OpenRecent(string path)
	{
		try
		{
			await FileSystemService.LaunchFileAsync(path);
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
		}
	}
}