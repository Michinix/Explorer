using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
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
}