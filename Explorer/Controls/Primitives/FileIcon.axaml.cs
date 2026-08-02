using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Explorer.Services;

namespace Explorer.Controls.Primitives;

public partial class FileIcon : UserControl
{
	public static readonly StyledProperty<bool> IsDirectoryProperty =
		AvaloniaProperty.Register<FileIcon, bool>(nameof(IsDirectory));

	public static readonly StyledProperty<string?> EntryPathProperty =
		AvaloniaProperty.Register<FileIcon, string?>(nameof(EntryPath));

	public static readonly StyledProperty<bool> UseThumbnailProperty =
		AvaloniaProperty.Register<FileIcon, bool>(nameof(UseThumbnail), true);

	public static readonly DirectProperty<FileIcon, Bitmap?> OsIconProperty =
		AvaloniaProperty.RegisterDirect<FileIcon, Bitmap?>(nameof(OsIcon), o => o.OsIcon);

	public static readonly DirectProperty<FileIcon, bool> HasOsIconProperty =
		AvaloniaProperty.RegisterDirect<FileIcon, bool>(nameof(HasOsIcon), o => o.HasOsIcon);

	private bool _hasOsIcon;
	private int _loadToken;

	private Bitmap? _osIcon;

	public FileIcon()
	{
		InitializeComponent();
	}

	public bool IsDirectory
	{
		get => GetValue(IsDirectoryProperty);
		set => SetValue(IsDirectoryProperty, value);
	}

	public string? EntryPath
	{
		get => GetValue(EntryPathProperty);
		set => SetValue(EntryPathProperty, value);
	}

	public bool UseThumbnail
	{
		get => GetValue(UseThumbnailProperty);
		set => SetValue(UseThumbnailProperty, value);
	}

	public Bitmap? OsIcon
	{
		get => _osIcon;
		private set => SetAndRaise(OsIconProperty, ref _osIcon, value);
	}

	public bool HasOsIcon
	{
		get => _hasOsIcon;
		private set => SetAndRaise(HasOsIconProperty, ref _hasOsIcon, value);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);

		if (change.Property == EntryPathProperty || change.Property == IsDirectoryProperty ||
		    change.Property == UseThumbnailProperty)
			_ = LoadIconAsync();
	}

	private async Task LoadIconAsync()
	{
		var token = ++_loadToken;
		var path = EntryPath;

		if (IsDirectory || string.IsNullOrEmpty(path))
		{
			OsIcon = null;
			HasOsIcon = false;
			return;
		}

		var bitmap = await IconService.GetIconAsync(path, UseThumbnail);

		if (token != _loadToken) return;

		OsIcon = bitmap;
		HasOsIcon = bitmap is not null;
	}
}