using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Explorer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        AddHandler(PointerPressedEvent, OnWindowPointerPressed, RoutingStrategies.Tunnel);
    }

    private void OnWindowPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var topLevel = GetTopLevel(this)!;
        topLevel.FocusManager.Focus(null);
    }
}