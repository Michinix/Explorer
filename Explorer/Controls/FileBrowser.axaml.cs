using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Messaging;
using Explorer.Models;
using Explorer.ViewModels;
using Explorer.Views;

namespace Explorer.Controls;

public partial class FileBrowser : UserControl
{
	private FileBrowserViewModel? _observed;

	public FileBrowser()
	{
		InitializeComponent();

		WeakReferenceMessenger.Default.Register<MoveRequestedMessage>(this,
			(_, message) => _ = ShowMoveToDialogAsync(message));
	}

	private async Task ShowMoveToDialogAsync(MoveRequestedMessage message)
	{
		if (DataContext is not FileBrowserViewModel vm) return;
		if (TopLevel.GetTopLevel(this) is not Window owner) return;

		var moveToViewModel = new MoveToViewModel(message.Entries, message.SourcePath);
		var window = new MoveToWindow
		{
			DataContext = moveToViewModel,
			Position = owner.Position,
			Width = owner.Bounds.Width,
			Height = owner.Bounds.Height
		};

		var destination = await window.ShowDialog<string?>(owner);

		if (!string.IsNullOrEmpty(destination))
			await vm.FileOps.MoveEntriesAsync(message.Entries, destination);
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

	private void OnTilePointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if (sender is not Control { DataContext: FileSystemEntry entry } control) return;
		if (DataContext is not FileBrowserViewModel vm) return;

		if (!e.GetCurrentPoint(control).Properties.IsLeftButtonPressed) return;

		var isMultiSelect = OperatingSystem.IsMacOS()
			? e.KeyModifiers.HasFlag(KeyModifiers.Meta)
			: e.KeyModifiers.HasFlag(KeyModifiers.Control);

		if (isMultiSelect)
		{
			entry.IsSelected = !entry.IsSelected;
		}
		else
		{
			foreach (var other in vm.Entries)
				other.IsSelected = false;

			entry.IsSelected = true;
		}

		vm.SelectedEntry = entry;
		e.Handled = true;
	}

	private Point? _selectionOrigin;
	private HashSet<FileSystemEntry> _selectionBaseline = [];

	private void OnTilesBackgroundPointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if (sender is not Control listBox) return;
		if (!e.GetCurrentPoint(listBox).Properties.IsLeftButtonPressed) return;
		if (DataContext is not FileBrowserViewModel vm) return;

		var isAdditive = OperatingSystem.IsMacOS()
			? e.KeyModifiers.HasFlag(KeyModifiers.Meta)
			: e.KeyModifiers.HasFlag(KeyModifiers.Control);

		_selectionBaseline = isAdditive
			? vm.Entries.Where(entry => entry.IsSelected).ToHashSet()
			: [];

		if (!isAdditive)
			foreach (var entry in vm.Entries)
				entry.IsSelected = false;

		vm.SelectedEntry = null;

		_selectionOrigin = e.GetPosition(this);
		e.Pointer.Capture(listBox);

		SelectionBox.IsVisible = true;
		UpdateSelectionBox(_selectionOrigin.Value, _selectionOrigin.Value);

		e.Handled = true;
	}

	private void OnTilesPointerMoved(object? sender, PointerEventArgs e)
	{
		if (_selectionOrigin is not { } origin) return;
		if (DataContext is not FileBrowserViewModel vm) return;
		if (TilesListBox is null) return;

		var rect = UpdateSelectionBox(origin, e.GetPosition(this));

		foreach (var entry in vm.Entries)
		{
			if (TilesListBox.ContainerFromItem(entry) is not Control container) continue;

			var topLeft = container.TranslatePoint(new Point(0, 0), this) ?? new Point();
			var containerRect = new Rect(topLeft, container.Bounds.Size);

			entry.IsSelected = _selectionBaseline.Contains(entry) || rect.Intersects(containerRect);
		}
	}

	private void OnTilesPointerReleased(object? sender, PointerReleasedEventArgs e)
	{
		if (_selectionOrigin is null) return;

		e.Pointer.Capture(null);
		_selectionOrigin = null;
		_selectionBaseline = [];
		SelectionBox.IsVisible = false;
	}

	private Rect UpdateSelectionBox(Point origin, Point current)
	{
		var x = Math.Min(origin.X, current.X);
		var y = Math.Min(origin.Y, current.Y);
		var width = Math.Abs(current.X - origin.X);
		var height = Math.Abs(current.Y - origin.Y);
		var rect = new Rect(x, y, width, height);

		Canvas.SetLeft(SelectionBox, rect.X);
		Canvas.SetTop(SelectionBox, rect.Y);
		SelectionBox.Width = rect.Width;
		SelectionBox.Height = rect.Height;

		return rect;
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