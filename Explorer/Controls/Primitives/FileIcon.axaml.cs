using Avalonia;
using Avalonia.Controls;

namespace Explorer.Controls.Primitives;

public partial class FileIcon : UserControl
{
    public static readonly StyledProperty<bool> IsDirectoryProperty =
        AvaloniaProperty.Register<FileIcon, bool>(nameof(IsDirectory));

    public FileIcon()
    {
        InitializeComponent();
    }

    public bool IsDirectory
    {
        get => GetValue(IsDirectoryProperty);
        set => SetValue(IsDirectoryProperty, value);
    }
}