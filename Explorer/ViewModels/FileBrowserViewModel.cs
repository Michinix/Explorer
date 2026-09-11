using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Explorer.Models;
using Explorer.Services;

namespace Explorer.ViewModels;

public partial class FileBrowserViewModel : ViewModelBase
{
	private readonly ClipboardService _clipboard;
	private readonly NavigationService _navigation;
	private readonly OcrService _ocr;

	private readonly HashSet<string> _ocrPublishedPaths = new(StringComparer.OrdinalIgnoreCase);
	private readonly SettingsService _settings;

	private bool _activeSearchIsOcr;

	private string _activeSearchTerm = string.Empty;

	private readonly Dictionary<string, SearchSnapshot> _searchSnapshots = new(StringComparer.OrdinalIgnoreCase);
	private string _trackedPath;

	[ObservableProperty] [NotifyPropertyChangedFor(nameof(NoResultsFound))]
	private ObservableCollection<FileSystemEntry> _entries = [];

	[ObservableProperty] private bool _isDetailsPaneVisible;

	[ObservableProperty] [NotifyPropertyChangedFor(nameof(ViewToggleIconPath))]
	private bool _isGridView;

	[ObservableProperty] [NotifyPropertyChangedFor(nameof(NoResultsFound))]
	private bool _isLoading;

	[ObservableProperty] [NotifyPropertyChangedFor(nameof(SearchPlaceholder))]
	private bool _isOcrMode;

	[ObservableProperty] [NotifyPropertyChangedFor(nameof(NoResultsFound))]
	private bool _isOcrScanning;

	[ObservableProperty] [NotifyPropertyChangedFor(nameof(IsDetailsTab))]
	private bool _isPreviewTab;

	[ObservableProperty] private bool _isSearchBarOpen;

	[ObservableProperty] [NotifyPropertyChangedFor(nameof(NoResultsFound))]
	private bool _isSearchResult;

	private CancellationTokenSource? _ocrCts;

	[ObservableProperty] [NotifyPropertyChangedFor(nameof(OcrProgressText))]
	private int _ocrDone;

	[ObservableProperty] [NotifyPropertyChangedFor(nameof(OcrProgressText))]
	private int _ocrTotal;

	[ObservableProperty] private string _searchTerm = string.Empty;
	[ObservableProperty] private FileSystemEntry? _selectedEntry;

	public string AppVersion { get; } = BuildAppVersionText();

	public FileBrowserViewModel(NavigationService navigation, SettingsService settings, OcrService ocr,
		ClipboardService clipboard)
	{
		_navigation = navigation;
		_settings = settings;
		_ocr = ocr;
		_clipboard = clipboard;
		_isGridView = settings.IsGridView;
		_isDetailsPaneVisible = settings.IsDetailsPaneVisible;
		_trackedPath = navigation.CurrentPath;

		WeakReferenceMessenger.Default.Register<CurrentPathChangedMessage>(this, (r, m) =>
		{
			SaveSearchSnapshot(_trackedPath);
			_trackedPath = m.NewPath;

			CancelOcr();

			if (TryRestoreSearchSnapshot(m.NewPath))
				return;

			IsOcrMode = false;
			IsSearchBarOpen = false;
			SearchTerm = string.Empty;
			OnPropertyChanged(nameof(SearchPlaceholder));
			_ = LoadEntriesAsync();
		});

		_settings.PinnedItems.CollectionChanged += (_, _) => UpdatePinnedStates();
		_clipboard.StagedChanged += UpdateClipboardStates;
	}

	public string SearchPlaceholder => IsOcrMode
		? "Rechercher du texte dans les images..."
		: $"Rechercher dans : {GetFolderDisplayName(_navigation.CurrentPath)}";

	public string OcrProgressText => $"Analyse OCR : {OcrDone}/{OcrTotal}";

	public bool IsDetailsTab => !IsPreviewTab;

	public bool NoResultsFound => IsSearchResult && !IsLoading && !IsOcrScanning && Entries.Count == 0;

	public string ViewToggleIconPath => IsGridView ? "/Assets/Icons/List.svg" : "/Assets/Icons/Grid.svg";

	public FileOperationsViewModel FileOps { get; set; } = null!;

	public bool AllSelected
	{
		get => Entries.Count > 0 && Entries.All(e => e.IsSelected);
		set
		{
			foreach (var entry in Entries)
				entry.IsSelected = value;
			OnPropertyChanged();
		}
	}

	private int SelectedItemCount => Entries.Count(e => e.IsSelected);

	public string SelectedItemCountText => SelectedItemCount switch
	{
		0 or 1 => $"{SelectedItemCount} élément sélectionné",
		_ => $"{SelectedItemCount} éléments sélectionnés"
	};

	public string ItemCountText => Entries.Count switch
	{
		0 => "Aucun élément",
		1 => "1 élément",
		_ => $"{Entries.Count} éléments"
	};

	public IReadOnlyList<FileSystemEntry> SelectionTargets
	{
		get
		{
			var selected = Entries.Where(e => e is { IsSelected: true, IsNew: false }).ToArray();
			if (selected.Length > 0)
				return selected;

			return SelectedEntry is { IsNew: false } ? [SelectedEntry] : [];
		}
	}

	public bool HasSelectionTargets => SelectionTargets.Count > 0;

	public bool CanOpenInVsCode => ExternalToolsService.IsVsCodeAvailable;

	private static string BuildAppVersionText()
	{
		var version = Assembly.GetEntryAssembly()?
			.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
			.InformationalVersion;

		if (string.IsNullOrWhiteSpace(version))
			return "Explorer";

		return $"Explorer v{version}";
	}

	private void UpdatePinnedStates()
	{
		foreach (var entry in Entries)
			entry.IsPinned = entry.IsDirectory && _settings.IsPinned(entry.FullPath);
	}

	private void UpdateClipboardStates()
	{
		foreach (var entry in Entries)
			ApplyClipboardState(entry);
	}

	private void ApplyClipboardState(FileSystemEntry entry)
	{
		var staged = !entry.IsNew && _clipboard.IsStaged(entry.FullPath);

		entry.IsCut = staged && _clipboard.Operation == ClipboardOperation.Cut;
		entry.IsCopied = staged && _clipboard.Operation == ClipboardOperation.Copy;
	}

	partial void OnEntriesChanged(
		ObservableCollection<FileSystemEntry>? oldValue,
		ObservableCollection<FileSystemEntry> newValue
	)
	{
		if (oldValue != null)
			foreach (var entry in oldValue)
				entry.PropertyChanged -= OnEntryPropertyChanged;

		foreach (var entry in newValue)
			AttachEntry(entry);

		NotifySelectionChanged();
	}

	private void AttachEntry(FileSystemEntry entry)
	{
		entry.IsSelected = false;
		entry.IsPinned = entry.IsDirectory && _settings.IsPinned(entry.FullPath);
		ApplyClipboardState(entry);
		entry.PropertyChanged += OnEntryPropertyChanged;

		entry.HasAppeared = false;
		Dispatcher.UIThread.Post(() => entry.HasAppeared = true, DispatcherPriority.Background);
	}

	partial void OnSelectedEntryChanged(FileSystemEntry? value)
	{
		NotifySelectionChanged();
	}

	private void OnEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != nameof(FileSystemEntry.IsSelected)) return;

		NotifySelectionChanged();
	}

	private void NotifySelectionChanged()
	{
		OnPropertyChanged(nameof(AllSelected));
		OnPropertyChanged(nameof(SelectedItemCount));
		OnPropertyChanged(nameof(SelectedItemCountText));
		OnPropertyChanged(nameof(ItemCountText));
		OnPropertyChanged(nameof(SelectionTargets));
		OnPropertyChanged(nameof(HasSelectionTargets));

		FileOps?.NotifySelectionChanged();
	}

	private void SaveSearchSnapshot(string path)
	{
		if (!IsSearchResult || IsOcrScanning || string.IsNullOrWhiteSpace(SearchTerm))
		{
			_searchSnapshots.Remove(path);
			return;
		}

		_searchSnapshots[path] = new SearchSnapshot(SearchTerm, IsOcrMode, IsSearchBarOpen, Entries);
	}

	private bool TryRestoreSearchSnapshot(string path)
	{
		if (!_searchSnapshots.TryGetValue(path, out var snapshot))
			return false;

		_activeSearchTerm = snapshot.SearchTerm;
		_activeSearchIsOcr = snapshot.IsOcrMode;

		IsOcrMode = snapshot.IsOcrMode;
		IsSearchBarOpen = snapshot.IsSearchBarOpen;
		SearchTerm = snapshot.SearchTerm;
		IsSearchResult = true;
		SelectedEntry = null;
		Entries = snapshot.Entries;

		OnPropertyChanged(nameof(SearchPlaceholder));
		return true;
	}

	public async Task LoadEntriesAsync()
	{
		CancelOcr();

		IsSearchResult = false;
		SelectedEntry = null;
		_activeSearchTerm = string.Empty;
		_activeSearchIsOcr = false;

		Entries = [];

		if (string.Equals(_navigation.CurrentPath, NavigationService.HomePath, StringComparison.OrdinalIgnoreCase))
		{
			IsLoading = false;
			return;
		}

		IsLoading = true;

		try
		{
			var loaded = await FileSystemService.ListEntriesAsync(_navigation.CurrentPath);
			Entries = new ObservableCollection<FileSystemEntry>(loaded);
		}
		catch (UnauthorizedAccessException ex)
		{
			Debug.WriteLine(ex.Message);
		}
		catch (DirectoryNotFoundException ex)
		{
			Debug.WriteLine(ex.Message);
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
		}
		finally
		{
			IsLoading = false;
		}
	}

	[RelayCommand]
	private async Task ClearSearch()
	{
		CancelOcr();
		IsOcrMode = false;
		SearchTerm = string.Empty;
		_searchSnapshots.Remove(_navigation.CurrentPath);

		if (IsSearchResult)
			await LoadEntriesAsync();
	}

	[RelayCommand]
	private async Task ToggleOcrMode()
	{
		if (IsOcrMode)
		{
			await ClearSearch();
			IsSearchBarOpen = false;
			return;
		}

		IsOcrMode = true;
		IsSearchBarOpen = true;
	}

	[RelayCommand]
	private void ShowDetailsTab()
	{
		IsPreviewTab = false;
	}

	[RelayCommand]
	private void ShowPreviewTab()
	{
		IsPreviewTab = true;
	}

	[RelayCommand]
	private void CancelOcr()
	{
		try
		{
			_ocrCts?.Cancel();
		}
		catch (ObjectDisposedException ex)
		{
			Debug.WriteLine(ex.Message);
		}
	}

	[RelayCommand]
	private async Task Search()
	{
		if (_navigation.IsHome)
			return;

		if (string.IsNullOrWhiteSpace(SearchTerm))
		{
			if (IsSearchResult)
				await LoadEntriesAsync();
			return;
		}

		if (IsSearchResult && _activeSearchIsOcr == IsOcrMode &&
		    string.Equals(SearchTerm, _activeSearchTerm, StringComparison.OrdinalIgnoreCase))
			return;

		_activeSearchTerm = SearchTerm;
		_activeSearchIsOcr = IsOcrMode;

		if (IsOcrMode)
		{
			await OcrSearchAsync(SearchTerm);
			return;
		}

		IsLoading = true;
		IsSearchResult = true;
		SelectedEntry = null;

		try
		{
			Entries = new ObservableCollection<FileSystemEntry>(
				await FileSystemService.SearchEntriesAsync(_navigation.CurrentPath, SearchTerm));
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
		}
		finally
		{
			IsLoading = false;
		}
	}

	private async Task OcrSearchAsync(string term)
	{
		CancelOcr();

		var cts = new CancellationTokenSource();
		_ocrCts = cts;

		IsSearchResult = true;
		SelectedEntry = null;
		Entries = [];
		OcrDone = 0;
		OcrTotal = 0;
		IsOcrScanning = true;
		_ocrPublishedPaths.Clear();

		var results = Entries;
		var root = _navigation.CurrentPath;
		var processed = 0;

		try
		{
			var images = await FileSystemService.ListImagesRecursiveAsync(root);
			OcrTotal = images.Length;

			var options = new ParallelOptions
			{
				MaxDegreeOfParallelism = OcrService.Degree,
				CancellationToken = cts.Token
			};

			await Parallel.ForEachAsync(images, options, async (entry, token) =>
			{
				var text = await _ocr.ExtractTextAsync(entry.FullPath, token);
				var matched = OcrService.Matches(text, term, out var snippet);

				if (matched)
				{
					entry.OcrSnippet = snippet;
					entry.RelativeFolder = GetRelativeFolder(root, entry.FullPath);
				}

				var count = Interlocked.Increment(ref processed);

				Dispatcher.UIThread.Post(() => PublishOcrResult(results, matched ? entry : null, count));
			});
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
		}
		finally
		{
			IsOcrScanning = false;

			if (ReferenceEquals(_ocrCts, cts))
				_ocrCts = null;

			cts.Dispose();
		}
	}

	private void PublishOcrResult(ObservableCollection<FileSystemEntry> target, FileSystemEntry? match, int done)
	{
		if (!ReferenceEquals(Entries, target))
			return;

		if (match is not null && _ocrPublishedPaths.Add(match.FullPath))
		{
			AttachEntry(match);
			target.Add(match);
			NotifySelectionChanged();
			OnPropertyChanged(nameof(NoResultsFound));
		}

		OcrDone = done;
	}

	private static string GetRelativeFolder(string root, string fullPath)
	{
		try
		{
			var folder = Path.GetDirectoryName(fullPath);
			if (string.IsNullOrEmpty(folder))
				return "—";

			var relative = Path.GetRelativePath(root, folder);
			return relative == "." ? "—" : relative;
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
			return "—";
		}
	}

	private static string GetFolderDisplayName(string path)
	{
		var trimmed = path.TrimEnd('/', '\\');
		var folder = Path.GetFileName(trimmed);
		return string.IsNullOrEmpty(folder) ? path : folder;
	}

	[RelayCommand]
	private async Task EntryDoubleClicked()
	{
		if (SelectedEntry is null) return;

		if (SelectedEntry.IsDirectory)
		{
			_navigation.NavigateTo(SelectedEntry.FullPath);
			return;
		}

		await OpenSelectedFile();
	}

	[RelayCommand]
	private async Task OpenSelectedFile()
	{
		if (SelectedEntry is null || SelectedEntry.IsDirectory) return;

		try
		{
			await FileSystemService.LaunchFileAsync(SelectedEntry.FullPath);
			_settings.AddRecent(SelectedEntry.Name, SelectedEntry.FullPath);
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
		}
	}

	[RelayCommand]
	private void ToggleView()
	{
		IsGridView = !IsGridView;
		_settings.SetGridView(IsGridView);
	}

	[RelayCommand]
	private void ToggleDetailsPane()
	{
		IsDetailsPaneVisible = !IsDetailsPaneVisible;
		_settings.SetDetailsPaneVisible(IsDetailsPaneVisible);
	}

	[RelayCommand]
	private async Task OpenTerminalHere()
	{
		await ExternalToolsService.OpenTerminalAsync(_navigation.CurrentPath);
	}

	[RelayCommand]
	private async Task OpenInVsCode()
	{
		await ExternalToolsService.OpenInVsCodeAsync(_navigation.CurrentPath);
	}

	public void AddDraft(bool isDirectory)
	{
		var draft = new FileSystemEntry(string.Empty, string.Empty, string.Empty, isDirectory, "—", DateTime.Now,
			DateTime.Now)
		{
			IsNew = true,
			IsEditing = true
		};

		draft.PropertyChanged += OnEntryPropertyChanged;
		Entries.Add(draft);
		SelectedEntry = draft;

		OnPropertyChanged(nameof(NoResultsFound));
	}

	public void Remove(FileSystemEntry entry)
	{
		Entries.Remove(entry);
	}

	private sealed record SearchSnapshot(
		string SearchTerm,
		bool IsOcrMode,
		bool IsSearchBarOpen,
		ObservableCollection<FileSystemEntry> Entries);
}