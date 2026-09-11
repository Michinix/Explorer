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

	private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if (DataContext is not HomeViewModel vm) return;

		var navigation = vm.NavBar.Navigation;
		var properties = e.GetCurrentPoint(this).Properties;

		if (properties.IsXButton1Pressed && navigation.GoBackCommand.CanExecute(null))
		{
			navigation.GoBackCommand.Execute(null);
			e.Handled = true;
		}
		else if (properties.IsXButton2Pressed && navigation.GoForwardCommand.CanExecute(null))
		{
			navigation.GoForwardCommand.Execute(null);
			e.Handled = true;
		}
	}
}
