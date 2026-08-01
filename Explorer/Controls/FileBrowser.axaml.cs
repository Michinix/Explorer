using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Explorer.Models;
using Explorer.ViewModels;

namespace Explorer.Controls;

public partial class FileBrowser : UserControl
{
	public FileBrowser()
	{
		InitializeComponent();
	}

	private void OnRowDoubleTapped(object? sender, TappedEventArgs e)
	{
		if (e.Source is not Visual visual || visual.FindAncestorOfType<DataGridRow>() is null)
			return;

		if (DataContext is FileBrowserViewModel vm)
			vm.EntryDoubleClickedCommand.Execute(null);
	}

	private void OnTileDoubleTapped(object? sender, TappedEventArgs e)
	{
		if (e.Source is not Visual visual || visual.FindAncestorOfType<ListBoxItem>() is null)
			return;

		if (DataContext is FileBrowserViewModel vm)
			vm.EntryDoubleClickedCommand.Execute(null);
	}

	private void OnDataGridLoadingRow(object? sender, DataGridRowEventArgs e)
	{
		e.Row.ContextRequested -= OnEntryContextRequested;
		e.Row.ContextRequested += OnEntryContextRequested;
	}

	private void OnEntryContextRequested(object? sender, ContextRequestedEventArgs e)
	{
		if (sender is not Control { DataContext: FileSystemEntry entry }) return;
		if (DataContext is not FileBrowserViewModel vm) return;

		if (entry.IsSelected) return;

		foreach (var other in vm.Entries)
			other.IsSelected = false;

		vm.SelectedEntry = entry;
	}

	private void OnRenameTextBoxLoaded(object? sender, RoutedEventArgs e)
	{
		if (sender is not TextBox textBox) return;

		if (textBox.IsEffectivelyVisible)
		{
			textBox.Focus();
			textBox.SelectAll();
		}

		textBox.PropertyChanged += (_, args) =>
		{
			if (args.Property != IsVisibleProperty || !textBox.IsEffectivelyVisible) return;

			textBox.Focus();
			textBox.SelectAll();
		};
	}

	private void OnRenameTextBoxLostFocus(object? sender, RoutedEventArgs e)
	{
		if (sender is not TextBox { DataContext: FileSystemEntry { IsEditing: true } entry }) return;
		if (DataContext is not FileBrowserViewModel vm) return;

		vm.FileOps.CommitRenameCommand.Execute(entry);
	}
}