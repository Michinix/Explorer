using System;
using System.Collections.Generic;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Explorer.Services;

public partial class NavigationService : ObservableObject
{
    private readonly Stack<string> _backStack = new();
    private readonly Stack<string> _forwardStack = new();

    [ObservableProperty] private string _currentPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public bool CanGoBack => _backStack.Count > 0;
    public bool CanGoForward => _forwardStack.Count > 0;
    public bool CanGoUp => Directory.GetParent(CurrentPath) is not null;
    
    public event Action<string>? PathChanged;
    
    partial void OnCurrentPathChanged(string value)
    {
        PathChanged?.Invoke(value);
    }

    public void NavigateTo(string path)
    {
        _backStack.Push(CurrentPath);
        _forwardStack.Clear();
        CurrentPath = path;
    }

    public void GoBack()
    {
        if (!CanGoBack) return;
        _forwardStack.Push(CurrentPath);
        CurrentPath = _backStack.Pop();
    }

    public void GoForward()
    {
        if (!CanGoForward) return;
        _backStack.Push(CurrentPath);
        CurrentPath = _forwardStack.Pop();
    }

    public void GoUp()
    {
        var parent = Directory.GetParent(CurrentPath);
        if (parent is null) return;
        NavigateTo(parent.FullName);
    }
}