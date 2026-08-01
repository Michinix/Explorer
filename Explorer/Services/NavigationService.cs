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
	public static readonly string HomePath = "\\Accueil";
	private readonly Stack<string> _backStack = new();
	private readonly Stack<string> _forwardStack = new();

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(CanGoBack), nameof(CanGoForward), nameof(CanGoUp), nameof(IsHome))]
	[NotifyCanExecuteChangedFor(nameof(GoBackCommand), nameof(GoForwardCommand), nameof(GoUpCommand))]
	private string _currentPath = HomePath;

	[ObservableProperty] private string _editablePath = HomePath;

	public bool CanGoBack => _backStack.Count > 0;
	public bool CanGoForward => _forwardStack.Count > 0;

	public bool IsHome => string.Equals(CurrentPath, HomePath, StringComparison.OrdinalIgnoreCase);

	public bool CanGoUp
	{
		get
		{
			if (string.Equals(CurrentPath, HomePath, StringComparison.OrdinalIgnoreCase)) return false;

			try
			{
				return Directory.GetParent(CurrentPath) is not null;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}

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

	[RelayCommand]
	private void RevertEditedPath()
	{
		EditablePath = CurrentPath;
	}

	[RelayCommand]
	private void NavigateToPath(string? path)
	{
		if (!string.IsNullOrWhiteSpace(path))
			NavigateTo(path);
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