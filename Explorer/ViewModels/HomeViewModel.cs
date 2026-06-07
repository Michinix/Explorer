using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Explorer.Models;
using Explorer.Services;

namespace Explorer.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly Stack<string> _backStack = new();
    private readonly Stack<string> _forwardStack = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoBackCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoForwardCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoUpCommand))]
    private string _currentPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    [ObservableProperty]
    private FileSystemEntry? _selectedEntry;

    [ObservableProperty]
    private ObservableCollection<FileSystemEntry> _entries = [];

    [RelayCommand]
    public async Task LoadEntries()
    {
        var result = await FileSystemService.ListEntries(CurrentPath);
        Entries = new ObservableCollection<FileSystemEntry>(result);
    }

    [RelayCommand]
    private async Task EntryDoubleClicked()
    {
        if (SelectedEntry is null || !SelectedEntry.IsDirectory) return;

        _backStack.Push(CurrentPath);
        _forwardStack.Clear();
        CurrentPath = SelectedEntry.FullPath;
        await LoadEntries();
    }

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private async Task GoBack()
    {
        _forwardStack.Push(CurrentPath);
        CurrentPath = _backStack.Pop();
        await LoadEntries();
    }

    private bool CanGoBack() => _backStack.Count > 0;

    [RelayCommand(CanExecute = nameof(CanGoForward))]
    private async Task GoForward()
    {
        _backStack.Push(CurrentPath);
        CurrentPath = _forwardStack.Pop();
        await LoadEntries();
    }

    private bool CanGoForward() => _forwardStack.Count > 0;
    
    [RelayCommand(CanExecute = nameof(CanGoUp))]
    private async Task GoUp()
    {
        var parent = Directory.GetParent(CurrentPath);
        if (parent is null) return;

        _backStack.Push(CurrentPath);
        _forwardStack.Clear();
        CurrentPath = parent.FullName;
        await LoadEntries();
    }

    private bool CanGoUp() => Directory.GetParent(CurrentPath) is not null;
}