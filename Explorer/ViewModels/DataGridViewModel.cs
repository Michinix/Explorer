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
    private readonly FileSystemService _fileSystemService;
    private readonly NavigationService _navigation;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(FolderCount), nameof(FileCount))]
    private ObservableCollection<FileSystemEntry> _entries = [];

    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private FileSystemEntry? _selectedEntry;

    [ObservableProperty] private string? _statusMessage;

    public DataGridViewModel(NavigationService navigation, FileSystemService fileSystemService)
    {
        _navigation = navigation;
        _fileSystemService = fileSystemService;

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

    partial void OnEntriesChanged(ObservableCollection<FileSystemEntry> value)
    {
        foreach (var entry in value)
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

        try
        {
            Entries = new ObservableCollection<FileSystemEntry>(
                await _fileSystemService.ListEntriesAsync(_navigation.CurrentPath));
        }
        catch (UnauthorizedAccessException)
        {
            StatusMessage = "Accès refusé à ce dossier.";
        }
        catch (DirectoryNotFoundException)
        {
            StatusMessage = "Ce dossier n'existe plus.";
        }
        catch (Exception)
        {
            StatusMessage = "Une erreur est survenue lors du chargement.";
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
            await _fileSystemService.LaunchFileAsync(SelectedEntry.FullPath);
        }
        catch (Exception)
        {
            StatusMessage = $"Impossible d'ouvrir '{SelectedEntry.Name}'.";
        }
    }
}