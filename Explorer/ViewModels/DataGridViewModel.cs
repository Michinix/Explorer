using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Explorer.Models;
using Explorer.Services;

namespace Explorer.ViewModels;

public partial class DataGridViewModel : ViewModelBase
{
    private readonly NavigationService _navigation;

    [ObservableProperty] private FileSystemEntry? _selectedEntry;
    [ObservableProperty] private ObservableCollection<FileSystemEntry> _entries = [];

    public DataGridViewModel(NavigationService navigation)
    {
        _navigation = navigation;
        _navigation.PathChanged += OnPathChanged;
    }

    private void OnPathChanged(string path)
    {
        FireAndForget(LoadEntriesAsync());
    }

    public async Task LoadEntriesAsync()
    {
        var result = await FileSystemService.ListEntries(_navigation.CurrentPath);
        Entries = new ObservableCollection<FileSystemEntry>(result);
    }

    [RelayCommand]
    private void EntryDoubleClicked()
    {
        if (SelectedEntry is null || !SelectedEntry.IsDirectory) return;
        _navigation.NavigateTo(SelectedEntry.FullPath);
    }
}