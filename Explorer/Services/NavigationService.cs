using System;
using System.Collections.Generic;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Explorer.Models;

namespace Explorer.Services;

public partial class NavigationService : ObservableObject
{
    private readonly Stack<string> _backStack = new();
    private readonly Stack<string> _forwardStack = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoBack), nameof(CanGoForward), nameof(CanGoUp))]
    private string _currentPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public bool CanGoBack => _backStack.Count > 0;
    public bool CanGoForward => _forwardStack.Count > 0;
    public bool CanGoUp => Directory.GetParent(CurrentPath) is not null;
    
    partial void OnCurrentPathChanged(string value)
    {
        WeakReferenceMessenger.Default.Send(new CurrentPathChangedMessage(value));
    }

    public void NavigateTo(string path)
    {
        _backStack.Push(CurrentPath);
        _forwardStack.Clear();
        CurrentPath = path;
    }
    
    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack()
    {
        if (!CanGoBack) return;
        var targetPath = _backStack.Pop(); 
        _forwardStack.Push(CurrentPath); 
        CurrentPath = targetPath; 
    }

    [RelayCommand(CanExecute = nameof(CanGoForward))]
    private void GoForward()
    {
        if (!CanGoForward) return;
    
        var targetPath = _forwardStack.Pop();
        _backStack.Push(CurrentPath);
        CurrentPath = targetPath;
    }
    
    [RelayCommand(CanExecute = nameof(CanGoUp))]
    private void GoUp()
    {
        var parent = Directory.GetParent(CurrentPath);
        if (parent is null) return;
        NavigateTo(parent.FullName);
    }
}