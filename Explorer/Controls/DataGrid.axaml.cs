using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Explorer.Models;
using Explorer.ViewModels;

namespace Explorer.Controls;

public partial class DataGrid : UserControl
{
    public DataGrid()
    {
        InitializeComponent();
    }

    private void OnRowDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (e.Source is not Visual visual || visual.FindAncestorOfType<DataGridRow>() is null)
            return;

        if (DataContext is DataGridViewModel vm)
            vm.EntryDoubleClickedCommand.Execute(null);
    }

    private void OnDataGridKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Space || e.Source is TextBox) return;
        if (DataContext is not DataGridViewModel vm) return;

        vm.TogglePreviewCommand.Execute(null);
        e.Handled = true;
    }

    private void OnRenameTextBoxLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox) return;

        if (textBox.IsVisible)
        {
            textBox.Focus();
            textBox.SelectAll();
        }

        textBox.PropertyChanged += (_, args) =>
        {
            if (args.Property != IsVisibleProperty || !textBox.IsVisible) return;

            textBox.Focus();
            textBox.SelectAll();
        };
    }

    private void OnRenameTextBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextBox { DataContext: FileSystemEntry { IsEditing: true } entry }) return;
        if (DataContext is not DataGridViewModel vm) return;

        vm.CommitRenameCommand.Execute(entry);
    }
}