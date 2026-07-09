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

    public int FolderCount => Entries.Count(e => e.IsDirectory);
    public int FileCount => Entries.Count(e => !e.IsDirectory);
    public int SelectedItemCount => Entries.Count(e => e.IsSelected);

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

    partial void OnSelectedEntryChanged(FileSystemEntry? value)
    {
        if (value is null)
            IsPreviewOpen = false;

        OnPropertyChanged(nameof(SelectionTargets));
        OnPropertyChanged(nameof(HasSelectionTargets));
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
        OnPropertyChanged(nameof(SelectionTargets));
        OnPropertyChanged(nameof(HasSelectionTargets));
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

        OnPropertyChanged(nameof(FolderCount));
        OnPropertyChanged(nameof(FileCount));
    }

    public void Remove(FileSystemEntry entry)
    {
        Entries.Remove(entry);
    }
}