using System;
using System.ComponentModel;
using System.Linq;
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
	private FileBrowserViewModel? _observed;

	public FileBrowser()
	{
		InitializeComponent();
	}

	protected override void OnDataContextChanged(EventArgs e)
	{
		base.OnDataContextChanged(e);

		if (_observed is not null)
			_observed.PropertyChanged -= OnViewModelPropertyChanged;

		_observed = DataContext as FileBrowserViewModel;

		if (_observed is not null)
			_observed.PropertyChanged += OnViewModelPropertyChanged;

		ApplyOcrColumns();
	}

	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is not (nameof(FileBrowserViewModel.IsOcrMode) or
		    nameof(FileBrowserViewModel.IsSearchResult)))
			return;

		ApplyOcrColumns();
	}

	private void ApplyOcrColumns()
	{
		var showOcr = _observed is { IsOcrMode: true, IsSearchResult: true };

		SetColumnVisible("TEXTE TROUVÉ", showOcr);
		SetColumnVisible("DOSSIER", showOcr);
		SetColumnVisible("TAILLE", !showOcr);
		SetColumnVisible("TYPE", !showOcr);
		SetColumnVisible("MODIFIÉ LE", !showOcr);
	}

	private void SetColumnVisible(string header, bool isVisible)
	{
		var column = EntriesGrid.Columns.FirstOrDefault(c => Equals(c.Header, header));

		if (column is not null)
			column.IsVisible = isVisible;
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