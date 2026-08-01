using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace Explorer.Controls.Primitives;

public partial class NewItemButton : UserControl
{
	public static readonly StyledProperty<ICommand?> CreateFolderCommandProperty =
		AvaloniaProperty.Register<NewItemButton, ICommand?>(nameof(CreateFolderCommand));

	public static readonly StyledProperty<ICommand?> CreateFileCommandProperty =
		AvaloniaProperty.Register<NewItemButton, ICommand?>(nameof(CreateFileCommand));

	public NewItemButton()
	{
		InitializeComponent();
	}

	public ICommand? CreateFolderCommand
	{
		get => GetValue(CreateFolderCommandProperty);
		set => SetValue(CreateFolderCommandProperty, value);
	}

	public ICommand? CreateFileCommand
	{
		get => GetValue(CreateFileCommandProperty);
		set => SetValue(CreateFileCommandProperty, value);
	}
}