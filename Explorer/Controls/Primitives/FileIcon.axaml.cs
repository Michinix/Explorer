using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Explorer.Controls.Primitives;

public partial class FileIcon : UserControl
{
    public static readonly StyledProperty<IBrush?> IconBrushProperty =
        AvaloniaProperty.Register<FileIcon, IBrush?>(nameof(IconBrush));

    public static readonly StyledProperty<string?> IconLabelProperty =
        AvaloniaProperty.Register<FileIcon, string?>(nameof(IconLabel));

    public static readonly StyledProperty<bool> IsDirectoryProperty =
        AvaloniaProperty.Register<FileIcon, bool>(nameof(IsDirectory));

    public FileIcon()
    {
        InitializeComponent();
    }

    public IBrush? IconBrush
    {
        get => GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }

    public string? IconLabel
    {
        get => GetValue(IconLabelProperty);
        set => SetValue(IconLabelProperty, value);
    }

    public bool IsDirectory
    {
        get => GetValue(IsDirectoryProperty);
        set => SetValue(IsDirectoryProperty, value);
    }
}