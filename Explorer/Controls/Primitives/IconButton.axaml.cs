using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace Explorer.Controls.Primitives;

public partial class IconButton : UserControl
{
	public static readonly StyledProperty<string?> IconPathProperty =
		AvaloniaProperty.Register<IconButton, string?>(nameof(IconPath));

	public static readonly StyledProperty<ICommand?> CommandProperty =
		AvaloniaProperty.Register<IconButton, ICommand?>(nameof(Command));

	public IconButton()
	{
		InitializeComponent();
	}

	public string? IconPath
	{
		get => GetValue(IconPathProperty);
		set => SetValue(IconPathProperty, value);
	}

	public ICommand? Command
	{
		get => GetValue(CommandProperty);
		set => SetValue(CommandProperty, value);
	}
}
