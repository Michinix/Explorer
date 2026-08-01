using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace Explorer.Controls.Primitives;

public enum HomeCardVariant
{
	QuickAccess,
	Drive
}

public partial class HomeCard : UserControl
{
	private const double WarningThreshold = 60d;
	private const double CriticalThreshold = 85d;

	public static readonly StyledProperty<HomeCardVariant> VariantProperty =
		AvaloniaProperty.Register<HomeCard, HomeCardVariant>(nameof(Variant));

	public static readonly StyledProperty<string?> IconPathProperty =
		AvaloniaProperty.Register<HomeCard, string?>(nameof(IconPath));

	public static readonly StyledProperty<string?> IconCssProperty =
		AvaloniaProperty.Register<HomeCard, string?>(nameof(IconCss));

	public static readonly StyledProperty<bool> IsPinnedProperty =
		AvaloniaProperty.Register<HomeCard, bool>(nameof(IsPinned));

	public static readonly StyledProperty<string?> TextProperty =
		AvaloniaProperty.Register<HomeCard, string?>(nameof(Text));

	public static readonly StyledProperty<string?> DetailProperty =
		AvaloniaProperty.Register<HomeCard, string?>(nameof(Detail));

	public static readonly StyledProperty<double> ProgressProperty =
		AvaloniaProperty.Register<HomeCard, double>(nameof(Progress));

	public static readonly StyledProperty<ICommand?> CommandProperty =
		AvaloniaProperty.Register<HomeCard, ICommand?>(nameof(Command));

	public static readonly StyledProperty<object?> CommandParameterProperty =
		AvaloniaProperty.Register<HomeCard, object?>(nameof(CommandParameter));

	public static readonly DirectProperty<HomeCard, bool> IsDriveProperty =
		AvaloniaProperty.RegisterDirect<HomeCard, bool>(nameof(IsDrive), o => o.IsDrive);

	public static readonly DirectProperty<HomeCard, bool> IsProgressWarningProperty =
		AvaloniaProperty.RegisterDirect<HomeCard, bool>(nameof(IsProgressWarning), o => o.IsProgressWarning);

	public static readonly DirectProperty<HomeCard, bool> IsProgressCriticalProperty =
		AvaloniaProperty.RegisterDirect<HomeCard, bool>(nameof(IsProgressCritical), o => o.IsProgressCritical);

	private bool _isDrive;
	private bool _isProgressCritical;
	private bool _isProgressWarning;

	public HomeCard()
	{
		InitializeComponent();
	}

	public HomeCardVariant Variant
	{
		get => GetValue(VariantProperty);
		set => SetValue(VariantProperty, value);
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

	public bool IsPinned
	{
		get => GetValue(IsPinnedProperty);
		set => SetValue(IsPinnedProperty, value);
	}

	public string? Text
	{
		get => GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	public string? Detail
	{
		get => GetValue(DetailProperty);
		set => SetValue(DetailProperty, value);
	}

	public double Progress
	{
		get => GetValue(ProgressProperty);
		set => SetValue(ProgressProperty, value);
	}

	public ICommand? Command
	{
		get => GetValue(CommandProperty);
		set => SetValue(CommandProperty, value);
	}

	public object? CommandParameter
	{
		get => GetValue(CommandParameterProperty);
		set => SetValue(CommandParameterProperty, value);
	}

	public bool IsDrive
	{
		get => _isDrive;
		private set => SetAndRaise(IsDriveProperty, ref _isDrive, value);
	}

	public bool IsProgressWarning
	{
		get => _isProgressWarning;
		private set => SetAndRaise(IsProgressWarningProperty, ref _isProgressWarning, value);
	}

	public bool IsProgressCritical
	{
		get => _isProgressCritical;
		private set => SetAndRaise(IsProgressCriticalProperty, ref _isProgressCritical, value);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);

		if (change.Property == VariantProperty)
			IsDrive = Variant == HomeCardVariant.Drive;

		if (change.Property != ProgressProperty)
			return;

		IsProgressCritical = Progress >= CriticalThreshold;
		IsProgressWarning = Progress >= WarningThreshold && !IsProgressCritical;
	}
}