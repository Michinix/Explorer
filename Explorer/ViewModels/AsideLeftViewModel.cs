using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Explorer.Models;
using Explorer.Services;

namespace Explorer.ViewModels;

public partial class AsideLeftViewModel : ViewModelBase
{
	private readonly string _homePath = NavigationService.HomePath;
	private readonly NavigationService _navigation;
	private readonly SettingsService _settings;

	[ObservableProperty] [NotifyPropertyChangedFor(nameof(HasCloudStorage))]
	private ICollection<DriveItem> _cloudStorage = [];

	[ObservableProperty] [NotifyPropertyChangedFor(nameof(IsHomeActive))]
	private string _currentPath;

	[ObservableProperty] private ICollection<DriveItem> _drives = [];

	public AsideLeftViewModel(NavigationService navigation, SettingsService settings)
	{
		_navigation = navigation;
		_settings = settings;
		_currentPath = navigation.CurrentPath;

		WeakReferenceMessenger.Default.Register<CurrentPathChangedMessage>(this, (_, message) =>
			CurrentPath = message.NewPath);

		PinnedItems.CollectionChanged += (_, _) => UpdateActiveStates();

		_ = LoadDrivesAsync();
		_ = LoadCloudStorageAsync();
	}

	public bool HasCloudStorage => CloudStorage.Count > 0;

	public ObservableCollection<PinnedItem> PinnedItems => _settings.PinnedItems;

	public bool IsHomeActive => string.Equals(CurrentPath, _homePath, StringComparison.OrdinalIgnoreCase);

	private static bool IsUnder(string current, string basePath)
	{
		if (string.IsNullOrEmpty(current) || string.IsNullOrEmpty(basePath))
			return false;

		var separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
		var normalizedCurrent = current.TrimEnd(separators);
		var normalizedBase = basePath.TrimEnd(separators);

		if (string.Equals(normalizedCurrent, normalizedBase, StringComparison.OrdinalIgnoreCase))
			return true;

		return normalizedCurrent.StartsWith(normalizedBase + Path.DirectorySeparatorChar,
			StringComparison.OrdinalIgnoreCase);
	}

	partial void OnCurrentPathChanged(string value)
	{
		UpdateActiveStates();
	}

	private void UpdateActiveStates()
	{
		PinnedItem? bestPinnedMatch = null;
		if (!IsHomeActive)
			foreach (var pinned in PinnedItems)
				if (IsUnder(CurrentPath, pinned.FullPath) &&
				    (bestPinnedMatch is null || pinned.FullPath.Length > bestPinnedMatch.FullPath.Length))
					bestPinnedMatch = pinned;

		DriveItem? bestCloudMatch = null;
		foreach (var cloud in CloudStorage)
			if (IsUnder(CurrentPath, cloud.FullPath) &&
			    (bestCloudMatch is null || cloud.FullPath.Length > bestCloudMatch.FullPath.Length))
				bestCloudMatch = cloud;

		if (bestPinnedMatch is not null && bestCloudMatch is not null)
		{
			if (bestCloudMatch.FullPath.Length > bestPinnedMatch.FullPath.Length)
				bestPinnedMatch = null;
			else
				bestCloudMatch = null;
		}

		foreach (var pinned in PinnedItems)
			pinned.IsActive = pinned == bestPinnedMatch;

		var quickAccessActive = IsHomeActive || bestPinnedMatch is not null;

		var cloudActive = false;
		foreach (var cloud in CloudStorage)
		{
			cloud.IsActive = !quickAccessActive && cloud == bestCloudMatch;
			cloudActive |= cloud.IsActive;
		}

		foreach (var drive in Drives)
			drive.IsActive = !quickAccessActive && !cloudActive && IsUnder(CurrentPath, drive.FullPath);
	}

	private async Task LoadDrivesAsync()
	{
		Drives = await FileSystemService.GetDrivesAsync();
		UpdateActiveStates();
	}

	private async Task LoadCloudStorageAsync()
	{
		CloudStorage = await FileSystemService.GetCloudStorageAsync();
		UpdateActiveStates();
	}

	[RelayCommand]
	private void NavigateToDrive(string path)
	{
		_navigation.NavigateTo(path);
	}

	[RelayCommand]
	private void NavigateToHome()
	{
		_navigation.NavigateTo(_homePath);
	}

	[RelayCommand]
	private void RemovePinned(string path)
	{
		_settings.RemovePinned(path);
	}
}