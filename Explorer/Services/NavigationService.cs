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
    [NotifyCanExecuteChangedFor(nameof(GoBackCommand), nameof(GoForwardCommand), nameof(GoUpCommand))]
    private string _currentPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    [ObservableProperty]
    private string _editablePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public bool CanGoBack => _backStack.Count > 0;
    public bool CanGoForward => _forwardStack.Count > 0;
    public bool CanGoUp => Directory.GetParent(CurrentPath) is not null;

    partial void OnCurrentPathChanged(string value)
    {
        EditablePath = value;
        WeakReferenceMessenger.Default.Send(new CurrentPathChangedMessage(value));
    }

    public void NavigateTo(string path)
    {
        if (string.Equals(path, CurrentPath, StringComparison.OrdinalIgnoreCase))
            return;

        _backStack.Push(CurrentPath);
        _forwardStack.Clear();
        CurrentPath = path;
    }

    [RelayCommand]
    private void SubmitEditedPath()
    {
        if (!Directory.Exists(EditablePath)) EditablePath = CurrentPath;

        NavigateTo(EditablePath);
    }

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack()
    {
        _forwardStack.Push(CurrentPath);
        CurrentPath = _backStack.Pop();
    }

    [RelayCommand(CanExecute = nameof(CanGoForward))]
    private void GoForward()
    {
        _backStack.Push(CurrentPath);
        CurrentPath = _forwardStack.Pop();
    }

    [RelayCommand(CanExecute = nameof(CanGoUp))]
    private void GoUp()
    {
        NavigateTo(Directory.GetParent(CurrentPath)!.FullName);
    }

    [RelayCommand]
    private void Reload()
    {
        WeakReferenceMessenger.Default.Send(new CurrentPathChangedMessage(CurrentPath));
    }
}