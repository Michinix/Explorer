using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Explorer.Controls.Primitives;

public partial class Chrome : UserControl
{
	public Chrome()
	{
		InitializeComponent();
	}

	private Window? GetWindow()
	{
		return TopLevel.GetTopLevel(this) as Window;
	}

	private void CloseButton_Click(object? sender, RoutedEventArgs e)
	{
		GetWindow()?.Close();
	}

	private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
	{
		var window = GetWindow();
		if (window != null) window.WindowState = WindowState.Minimized;
	}

	private void MaximizeRestoreButton_Click(object? sender, RoutedEventArgs e)
	{
		var window = GetWindow();
		if (window != null && window.CanResize)
			window.WindowState = window.WindowState == WindowState.Maximized
				? WindowState.Normal
				: WindowState.Maximized;
	}

	private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) GetWindow()?.BeginMoveDrag(e);
	}
}