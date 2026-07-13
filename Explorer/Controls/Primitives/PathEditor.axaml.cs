using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace Explorer.Controls.Primitives;

public partial class PathEditor : UserControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<PathEditor, string?>(
            nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<ICommand?> SubmitCommandProperty =
        AvaloniaProperty.Register<PathEditor, ICommand?>(nameof(SubmitCommand));

    public static readonly StyledProperty<ICommand?> RevertCommandProperty =
        AvaloniaProperty.Register<PathEditor, ICommand?>(nameof(RevertCommand));

    public PathEditor()
    {
        InitializeComponent();
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public ICommand? SubmitCommand
    {
        get => GetValue(SubmitCommandProperty);
        set => SetValue(SubmitCommandProperty, value);
    }

    public ICommand? RevertCommand
    {
        get => GetValue(RevertCommandProperty);
        set => SetValue(RevertCommandProperty, value);
    }

    private void OnClearClick(object? sender, RoutedEventArgs e)
    {
        Text = string.Empty;
        InputBox.Focus();
    }

    private void OnInputLostFocus(object? sender, RoutedEventArgs e)
    {
        if (RevertCommand?.CanExecute(null) == true)
            RevertCommand.Execute(null);
    }
}