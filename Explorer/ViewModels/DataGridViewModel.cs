using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(FolderCount), nameof(FileCount))]
    private ObservableCollection<FileSystemEntry> _entries = [];

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isPreviewOpen;
    [ObservableProperty] private FileSystemEntry? _selectedEntry;

    public DataGridViewModel(NavigationService navigation)
    {
        _navigation = navigation;

        WeakReferenceMessenger.Default.Register<CurrentPathChangedMessage>(this,
            (r, m) => _ = LoadEntriesAsync());
    }

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

    public int FolderCount => Entries.Count(e => e.IsDirectory);
    public int FileCount => Entries.Count(e => !e.IsDirectory);
    public int SelectedItemCount => Entries.Count(e => e.IsSelected);

    partial void OnSelectedEntryChanged(FileSystemEntry? value)
    {
        if (value is null)
            IsPreviewOpen = false;
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
        {
            entry.IsSelected = false;
            entry.PropertyChanged += OnEntryPropertyChanged;
        }

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
    }

    public async Task LoadEntriesAsync()
    {
        IsLoading = true;
        SelectedEntry = null;

        try
        {
            Entries = new ObservableCollection<FileSystemEntry>(
                await FileSystemService.ListEntriesAsync(_navigation.CurrentPath));
        }
        catch (UnauthorizedAccessException)
        {
            // TODO Add Toast
        }
        catch (DirectoryNotFoundException)
        {
            // TODO Add Toast
        }
        catch (Exception)
        {
            // TODO Add Toast
        }
        finally
        {
            await Task.Delay(300);
            IsLoading = false;
        }
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
        catch (Exception)
        {
            // TODO Add Toast
        }
    }

    [RelayCommand]
    private void TogglePreview()
    {
        if (SelectedEntry is null || SelectedEntry.IsDirectory) return;

        IsPreviewOpen = !IsPreviewOpen;
    }

    [RelayCommand]
    private void CreateFile()
    {
        AddDraftEntry(false);
    }

    [RelayCommand]
    private void CreateFolder()
    {
        AddDraftEntry(true);
    }

    private void AddDraftEntry(bool isDirectory)
    {
        var draft = new FileSystemEntry(string.Empty, string.Empty, string.Empty, isDirectory, "—", DateTime.Now)
        {
            IsNew = true,
            IsEditing = true
        };

        draft.PropertyChanged += OnEntryPropertyChanged;
        Entries.Insert(0, draft);
        SelectedEntry = draft;

        OnPropertyChanged(nameof(FolderCount));
        OnPropertyChanged(nameof(FileCount));
    }

    [RelayCommand]
    private void Rename()
    {
        if (SelectedEntry is null) return;

        SelectedEntry.EditableName = SelectedEntry.Name;
        SelectedEntry.IsEditing = true;
    }

    [RelayCommand]
    private async Task CommitRename(FileSystemEntry entry)
    {
        entry.IsEditing = false;
        var name = entry.EditableName.Trim();

        if (entry.IsNew)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Entries.Remove(entry);
                return;
            }

            try
            {
                var path = Path.Combine(_navigation.CurrentPath, name);

                if (entry.IsDirectory)
                    await FileSystemService.CreateDirectoryAsync(path);
                else
                    await FileSystemService.CreateFileAsync(path);

                await LoadEntriesAsync();
            }
            catch (Exception)
            {
                Entries.Remove(entry);
                // TODO Add Toast
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(name) || name == entry.Name) return;

        try
        {
            await FileSystemService.RenameAsync(entry, name);
            await LoadEntriesAsync();
        }
        catch (Exception)
        {
            // TODO Add Toast
        }
    }

    [RelayCommand]
    private void CancelRename(FileSystemEntry entry)
    {
        entry.IsEditing = false;

        if (entry.IsNew)
            Entries.Remove(entry);
    }

    [RelayCommand]
    private async Task DeleteSelected()
    {
        var selected = Entries.Where(e => e.IsSelected).ToArray();
        if (selected.Length == 0) return;

        try
        {
            await FileSystemService.DeleteEntriesAsync(selected);
            await LoadEntriesAsync();
        }
        catch (Exception)
        {
            // TODO Add Toast
        }
    }
}