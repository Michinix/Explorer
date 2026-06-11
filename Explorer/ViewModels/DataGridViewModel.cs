using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Explorer.Models;
using Explorer.Services;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;

namespace Explorer.ViewModels;

public partial class DataGridViewModel : ViewModelBase
{
    private readonly NavigationService _navigation;

    [ObservableProperty] private FileSystemEntry? _selectedEntry;
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(FolderCount), nameof(FileCount))]
    private ObservableCollection<FileSystemEntry> _entries = [];
    
    public int FolderCount => Entries.Count(e => e.IsDirectory);
    public int FileCount => Entries.Count(e => !e.IsDirectory);
    
    public DataGridViewModel(NavigationService navigation)
    {
        _navigation = navigation;
    
        WeakReferenceMessenger.Default.Register<CurrentPathChangedMessage>(this, async (r, m) =>
        {
            await LoadEntriesAsync();
        });
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