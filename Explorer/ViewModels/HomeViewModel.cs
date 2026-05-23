using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Explorer.Models;
using Explorer.Services;

namespace Explorer.ViewModels;

public partial class HomeViewModel(Browser browser, BrowserService browserService) : ViewModelBase
{
    private Stack<string> _backStack = new();
    private Stack<string> _forwardStack = new();

    public string User { get; } = browser.CurrentUser;
    public string InitialDirectory { get; } = browser.HomeDirectory;
    
    [ObservableProperty]
    private string currentPath = string.Empty;
    
    [ObservableProperty]
    private DirectoryInfo[]? directories;
    
    [ObservableProperty]
    private FileInfo[]? files;
    
    [ObservableProperty]
    private bool canGoBack;
    
    [ObservableProperty]
    private bool canGoForward;

    public async void Initialize()
    {
        CurrentPath = browser.HomeDirectory;
        await LoadDirectoryContents(browser.HomeDirectory);
        CanGoBack = false;
        CanGoForward = false;
    }

    [RelayCommand]
    public async Task NavigateToDirectory(DirectoryInfo? directory)
    {
        if (directory == null) return;
        await NavigateTo(directory.FullName);
    }

    [RelayCommand]
    public async Task NavigateToPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        
        if (BrowserService.IsValidPath(path))
        {
            await NavigateTo(path);
        }
    }

    [RelayCommand]
    public async Task GoBack()
    {
        if (!CanGoBack) return;

        _forwardStack.Push(CurrentPath);
        var previousPath = _backStack.Pop();
        CanGoBack = _backStack.Count > 0;
        CanGoForward = true;
        
        CurrentPath = previousPath;
        await LoadDirectoryContents(previousPath);
    }

    [RelayCommand]
    public async Task GoForward()
    {
        if (!CanGoForward) return;

        _backStack.Push(CurrentPath);
        var nextPath = _forwardStack.Pop();
        CanGoBack = true;
        CanGoForward = _forwardStack.Count > 0;
        
        CurrentPath = nextPath;
        await LoadDirectoryContents(nextPath);
    }

    [RelayCommand]
    public async Task GoHome()
    {
        await NavigateTo(browser.HomeDirectory);
    }

    private async Task NavigateTo(string path)
    {
        if (!BrowserService.IsValidPath(path)) return;
        
        if (CurrentPath != null)
        {
            _backStack.Push(CurrentPath);
            _forwardStack.Clear();
            CanGoBack = true;
            CanGoForward = false;
        }

        CurrentPath = path;
        await LoadDirectoryContents(path);
    }

    private async Task LoadDirectoryContents(string path)
    {
        Directories = await BrowserService.ListDirectories(path);
        Files = await BrowserService.ListFiles(path);
    }
}