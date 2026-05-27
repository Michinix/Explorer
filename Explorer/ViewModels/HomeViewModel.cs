using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Explorer.Models;
using Explorer.Services;

namespace Explorer.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _currentPath = FileSystem.HomeDirectory;

    [ObservableProperty]
    private ObservableCollection<FileSystemEntry> _entries = [];

    [RelayCommand]
    public async Task LoadEntries()
    {
        var result = await FileSystemService.ListEntries(CurrentPath);
        Entries = new ObservableCollection<FileSystemEntry>(result);
    }
}