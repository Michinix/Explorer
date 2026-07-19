using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Explorer.Models;
using Explorer.Services;

namespace Explorer.ViewModels;

public partial class DataGridViewModel : ViewModelBase
{
    private readonly NavigationService _navigation;

    private string _activeSearchTerm = string.Empty;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(NoResultsFound))]
    private ObservableCollection<FileSystemEntry> _entries = [];

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(NoResultsFound))]
    private bool _isLoading;

    [ObservableProperty] private bool _isPreviewOpen;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(NoResultsFound))]
    private bool _isSearchResult;

    [ObservableProperty] private string _searchTerm = string.Empty;
    [ObservableProperty] private FileSystemEntry? _selectedEntry;

    public DataGridViewModel(NavigationService navigation)
    {
        _navigation = navigation;

        WeakReferenceMessenger.Default.Register<CurrentPathChangedMessage>(this, (r, m) =>
        {
            SearchTerm = string.Empty;
            OnPropertyChanged(nameof(SearchPlaceholder));
            _ = LoadEntriesAsync();
        });
    }

    public string SearchPlaceholder => $"Rechercher dans : {GetFolderDisplayName(_navigation.CurrentPath)}";

    public bool NoResultsFound => IsSearchResult && !IsLoading && Entries.Count == 0;

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

    partial void OnEntriesChanged(
        ObservableCollection<FileSystemEntry>? oldValue,
        ObservableCollection<FileSystemEntry> newValue
    )
    {
        if (oldValue != null)
            foreach (var entry in oldValue)
                entry.PropertyChanged -= OnEntryPropertyChanged;

        foreach (var entry in newValue)
        {
            entry.IsSelected = false;
            entry.PropertyChanged += OnEntryPropertyChanged;
        }

        NotifySelectionChanged();
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

    public async Task LoadEntriesAsync()
    {
        IsLoading = true;
        IsSearchResult = false;
        SelectedEntry = null;
        _activeSearchTerm = string.Empty;

        try
        {
            Entries = new ObservableCollection<FileSystemEntry>(
                await FileSystemService.ListEntriesAsync(_navigation.CurrentPath));
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
            await Task.Delay(300);
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ClearSearch()
    {
        SearchTerm = string.Empty;

        if (IsSearchResult)
            await LoadEntriesAsync();
    }

    [RelayCommand]
    private async Task Search()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            if (IsSearchResult)
                await LoadEntriesAsync();
            return;
        }

        if (IsSearchResult && string.Equals(SearchTerm, _activeSearchTerm, StringComparison.OrdinalIgnoreCase))
            return;

        _activeSearchTerm = SearchTerm;
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
            await Task.Delay(300);
            IsLoading = false;
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
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    [RelayCommand]
    private void TogglePreview()
    {
        if (SelectedEntry is null || SelectedEntry.IsDirectory) return;

        var selectedEntryType = SelectedEntry.FullPath.Split('.').LastOrDefault()?.ToLower();

        ICollection<string> imagesType = ["png", "jpeg", "jpg", "gif", "bmp", "tiff", "webp"];

        if (selectedEntryType is null || !imagesType.Contains(selectedEntryType))
            return;

        IsPreviewOpen = !IsPreviewOpen;
    }

    public void AddDraft(bool isDirectory)
    {
        var draft = new FileSystemEntry(string.Empty, string.Empty, string.Empty, isDirectory, "—", DateTime.Now)
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
}