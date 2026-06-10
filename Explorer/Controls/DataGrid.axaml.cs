using Avalonia.Controls;
using Avalonia.Input;
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
        if (DataContext is DataGridViewModel vm)
            vm.EntryDoubleClickedCommand.Execute(null);
    }
}