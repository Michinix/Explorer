using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Explorer.Models;
using Explorer.ViewModels;

namespace Explorer.Views;

public partial class MoveToWindow : Window
{
	private MoveToViewModel? _viewModel;

	public MoveToWindow()
	{
		InitializeComponent();
	}

	protected override void OnDataContextChanged(EventArgs e)
	{
		base.OnDataContextChanged(e);

		if (_viewModel is not null)
		{
			_viewModel.Confirmed -= OnConfirmed;
			_viewModel.Cancelled -= OnCancelled;
		}

		_viewModel = DataContext as MoveToViewModel;

		if (_viewModel is not null)
		{
			_viewModel.Confirmed += OnConfirmed;
			_viewModel.Cancelled += OnCancelled;
		}
	}

	private void OnConfirmed(object? sender, string destination)
	{
		Close(destination);
	}

	private void OnCancelled(object? sender, EventArgs e)
	{
		Close(null);
	}

	private void OnOverlayPointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
			_viewModel?.CancelCommand.Execute(null);
	}

	private void OnDirectoryDoubleTapped(object? sender, TappedEventArgs e)
	{
		if (e.Source is not Visual visual || visual.FindAncestorOfType<ListBoxItem>() is not { } item)
			return;

		if (item.DataContext is not FileSystemEntry entry) return;
		if (DataContext is not MoveToViewModel vm) return;

		vm.EnterDirectoryCommand.Execute(entry);
	}
}
