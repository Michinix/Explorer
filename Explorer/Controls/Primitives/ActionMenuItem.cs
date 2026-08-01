using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using SvgControl = Avalonia.Svg.Skia.Svg;

namespace Explorer.Controls.Primitives;

public class ActionMenuItem : MenuItem
{
	public static readonly StyledProperty<string?> TextProperty =
		AvaloniaProperty.Register<ActionMenuItem, string?>(nameof(Text));

	public static readonly StyledProperty<string?> IconPathProperty =
		AvaloniaProperty.Register<ActionMenuItem, string?>(nameof(IconPath));

	public static readonly StyledProperty<string?> IconCssProperty =
		AvaloniaProperty.Register<ActionMenuItem, string?>(nameof(IconCss));

	private readonly SvgControl _icon = new(new Uri("avares://Explorer/"))
	{
		Width = 14,
		Height = 14,
		VerticalAlignment = VerticalAlignment.Center
	};

	private readonly TextBlock _text = new();

	public ActionMenuItem()
	{
		var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
		panel.Children.Add(_icon);
		panel.Children.Add(_text);

		Header = panel;
	}

	protected override Type StyleKeyOverride => typeof(MenuItem);

	public string? Text
	{
		get => GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	public string? IconPath
	{
		get => GetValue(IconPathProperty);
		set => SetValue(IconPathProperty, value);
	}

	public string? IconCss
	{
		get => GetValue(IconCssProperty);
		set => SetValue(IconCssProperty, value);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);

		if (change.Property == TextProperty)
			_text.Text = Text;
		else if (change.Property == IconPathProperty)
			_icon.Path = IconPath;
		else if (change.Property == IconCssProperty)
			_icon.SetValue(SvgControl.CssProperty, IconCss);
	}
}