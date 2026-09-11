using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Explorer.Models;

public partial class FileSystemEntry(
	string name,
	string fullPath,
	string type,
	bool isDirectory,
	string displaySize,
	DateTime lastModified,
	DateTime lastAccessed
) : ObservableObject
{
	private static readonly HashSet<string> ImageTypes = new(StringComparer.OrdinalIgnoreCase)
		{ "PNG", "JPG", "JPEG", "BMP", "GIF", "WEBP", "ICO", "TIF", "TIFF" };

	[ObservableProperty] private string _editableName = name;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(ClipboardOpacity))]
	[NotifyPropertyChangedFor(nameof(EffectiveOpacity))]
	[NotifyPropertyChangedFor(nameof(IsClipboardStaged))]
	[NotifyPropertyChangedFor(nameof(ClipboardIconPath))]
	[NotifyPropertyChangedFor(nameof(ClipboardIconCss))]
	private bool _isCopied;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(ClipboardOpacity))]
	[NotifyPropertyChangedFor(nameof(EffectiveOpacity))]
	[NotifyPropertyChangedFor(nameof(IsClipboardStaged))]
	[NotifyPropertyChangedFor(nameof(ClipboardIconPath))]
	[NotifyPropertyChangedFor(nameof(ClipboardIconCss))]
	private bool _isCut;

	[ObservableProperty] private bool _isEditing;
	[ObservableProperty] private bool _isNew;
	[ObservableProperty] private bool _isPinned;
	[ObservableProperty] private bool _isSelected;

	[ObservableProperty] [NotifyPropertyChangedFor(nameof(EffectiveOpacity))]
	private bool _hasAppeared = true;

	[ObservableProperty] [NotifyPropertyChangedFor(nameof(HasOcrSnippet))]
	private string? _ocrSnippet;

	[ObservableProperty] private string? _relativeFolder;

	public string Name { get; } = name;
	public string FullPath { get; } = fullPath;
	public string Type { get; } = type;
	public bool IsDirectory { get; } = isDirectory;
	public string DisplaySize { get; } = displaySize;
	public DateTime LastModified { get; } = lastModified;
	public DateTime LastAccessed { get; } = lastAccessed;

	public string DisplayType => IsDirectory ? "DOSSIER" : $"Fichier {Type}";
	public string TypeLabel => IsDirectory ? "DOSSIER" : Type;
	public bool IsImage => !IsDirectory && ImageTypes.Contains(Type);
	public bool HasOcrSnippet => !string.IsNullOrEmpty(OcrSnippet);
	public bool ShowPinAction => IsDirectory && !IsPinned;
	public bool ShowUnpinAction => IsDirectory && IsPinned;

	public bool IsClipboardStaged => IsCut || IsCopied;
	public double ClipboardOpacity => IsCut ? 0.4 : 1;
	public double EffectiveOpacity => HasAppeared ? ClipboardOpacity : 0;
	public string ClipboardIconPath => IsCut ? "/Assets/Icons/Cut.svg" : "/Assets/Icons/Copy.svg";
	public string ClipboardIconCss => IsCut ? "path { fill: #F5A623 }" : "path { fill: #00AAFF }";

	partial void OnIsPinnedChanged(bool value)
	{
		OnPropertyChanged(nameof(ShowPinAction));
		OnPropertyChanged(nameof(ShowUnpinAction));
	}
}