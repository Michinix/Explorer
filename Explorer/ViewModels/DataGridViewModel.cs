using System.Collections.ObjectModel;
using System.ComponentModel;
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

    partial void OnEntriesChanged(ObservableCollection<FileSystemEntry> value)
    {
        foreach (var entry in value)
        {
            entry.IsSelected = false;
            entry.PropertyChanged += OnEntryPropertyChanged;
        }

        OnPropertyChanged(nameof(AllSelected));
    }

    private void OnEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FileSystemEntry.IsSelected))
            OnPropertyChanged(nameof(AllSelected));
    }

    public async Task LoadEntriesAsync()
    {
        IsLoading = true;

        var result = await FileSystemService.ListEntriesAsync(_navigation.CurrentPath);
        Entries = new ObservableCollection<FileSystemEntry>(result);

        await Task.Delay(300);
        IsLoading = false;
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

        await FileSystemService.LaunchFileAsync(SelectedEntry.FullPath);
    }
}