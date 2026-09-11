using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Explorer.Models;
using Explorer.Services;

namespace Explorer.ViewModels;

public partial class MoveToViewModel : ViewModelBase
{
	private readonly Stack<string> _backStack = new();
	private readonly HashSet<string> _blockedDestinations = new(StringComparer.OrdinalIgnoreCase);
	private readonly IReadOnlyList<FileSystemEntry> _entries;

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(ConfirmCommand), nameof(GoUpCommand))]
	[NotifyPropertyChangedFor(nameof(Destination))]
	private string _currentPath;

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
	[NotifyPropertyChangedFor(nameof(Destination), nameof(ConfirmLabel))]
	private FileSystemEntry? _selectedEntry;

	[ObservableProperty] private ObservableCollection<FileSystemEntry> _directories = [];
	[ObservableProperty] private ICollection<DriveItem> _cloudStorage = [];
	[ObservableProperty] private ICollection<DriveItem> _drives = [];
	[ObservableProperty] private bool _isLoading;

	public MoveToViewModel(IReadOnlyList<FileSystemEntry> entries, string startPath)
	{
		_entries = entries;

		foreach (var entry in entries)
		{
			_blockedDestinations.Add(entry.FullPath);

			var parent = Path.GetDirectoryName(entry.FullPath);
			if (!string.IsNullOrEmpty(parent))
				_blockedDestinations.Add(parent);
		}

		_currentPath = Directory.Exists(startPath) ? startPath : Path.GetPathRoot(startPath) ?? startPath;

		_ = LoadDrivesAsync();
		_ = LoadDirectoriesAsync();
	}

	public string Title => _entries.Count == 1
		? $"Déplacer « {_entries[0].Name} »"
		: $"Déplacer {_entries.Count} éléments";

	public string Destination => SelectedEntry?.FullPath ?? CurrentPath;

	public string ConfirmLabel => SelectedEntry is not null
		? $"Déplacer dans « {SelectedEntry.Name} »"
		: "Déplacer ici";

	public bool CanConfirm => !string.IsNullOrEmpty(Destination) && !IsBlockedDestination(Destination);

	public bool CanGoBack => _backStack.Count > 0;

	public bool CanGoUp
	{
		get
		{
			try
			{
				return Directory.GetParent(CurrentPath) is not null;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}

	public event EventHandler<string>? Confirmed;
	public event EventHandler? Cancelled;

	private bool IsBlockedDestination(string path)
	{
		if (_blockedDestinations.Contains(path))
			return true;

		return _entries.Any(e => e.IsDirectory && IsSameOrDescendant(path, e.FullPath));
	}

	private static bool IsSameOrDescendant(string path, string basePath)
	{
		var separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
		var normalizedPath = path.TrimEnd(separators);
		var normalizedBase = basePath.TrimEnd(separators);

		if (string.Equals(normalizedPath, normalizedBase, StringComparison.OrdinalIgnoreCase))
			return true;

		return normalizedPath.StartsWith(normalizedBase + Path.DirectorySeparatorChar,
			StringComparison.OrdinalIgnoreCase);
	}

	partial void OnCurrentPathChanged(string value)
	{
		SelectedEntry = null;
		_ = LoadDirectoriesAsync();
	}

	private async Task LoadDirectoriesAsync()
	{
		var path = CurrentPath;
		IsLoading = true;

		try
		{
			var result = await FileSystemService.ListDirectoriesAsync(path);
			if (path == CurrentPath)
			{
				Directories = new ObservableCollection<FileSystemEntry>(result);
				foreach (var directory in result)
					_ = LoadPreviewThumbnailAsync(directory, path);
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
			if (path == CurrentPath)
				Directories = [];
		}
		finally
		{
			if (path == CurrentPath)
				IsLoading = false;
		}
	}

	private async Task LoadPreviewThumbnailAsync(FileSystemEntry directory, string requestedForPath)
	{
		var imagePath = await FileSystemService.FindFirstImageAsync(directory.FullPath);
		if (imagePath is null || requestedForPath != CurrentPath) return;

		try
		{
			await using var stream = File.OpenRead(imagePath);
			directory.PreviewThumbnail = Bitmap.DecodeToWidth(stream, 40);
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
		}
	}

	private async Task LoadDrivesAsync()
	{
		Drives = await FileSystemService.GetDrivesAsync();
		CloudStorage = await FileSystemService.GetCloudStorageAsync();
	}

	private void NavigateAndTrack(string destination)
	{
		if (string.Equals(destination, CurrentPath, StringComparison.OrdinalIgnoreCase))
			return;

		_backStack.Push(CurrentPath);
		CurrentPath = destination;
		GoBackCommand.NotifyCanExecuteChanged();
	}

	[RelayCommand]
	private void NavigateTo(string? path)
	{
		if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
			NavigateAndTrack(path);
	}

	[RelayCommand]
	private void EnterDirectory(FileSystemEntry entry)
	{
		if (entry.IsDirectory)
			NavigateAndTrack(entry.FullPath);
	}

	[RelayCommand(CanExecute = nameof(CanGoUp))]
	private void GoUp()
	{
		var parent = Directory.GetParent(CurrentPath);
		if (parent is not null)
			NavigateAndTrack(parent.FullName);
	}

	[RelayCommand(CanExecute = nameof(CanGoBack))]
	private void GoBack()
	{
		if (_backStack.Count == 0) return;

		CurrentPath = _backStack.Pop();
		GoBackCommand.NotifyCanExecuteChanged();
	}

	[RelayCommand(CanExecute = nameof(CanConfirm))]
	private void Confirm()
	{
		Confirmed?.Invoke(this, Destination);
	}

	[RelayCommand]
	private void Cancel()
	{
		Cancelled?.Invoke(this, EventArgs.Empty);
	}
}
