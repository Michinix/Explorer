using Avalonia.Controls;
using Avalonia.Input;
using Explorer.ViewModels;

namespace Explorer.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
    }
    
    private void OnRowDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is HomeViewModel vm)
            vm.EntryDoubleClickedCommand.Execute(null);
    }
}