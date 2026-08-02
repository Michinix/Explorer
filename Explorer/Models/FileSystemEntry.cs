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
	[ObservableProperty] private bool _isEditing;
	[ObservableProperty] private bool _isNew;
	[ObservableProperty] private bool _isPinned;
	[ObservableProperty] private bool _isSelected;

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

	partial void OnIsPinnedChanged(bool value)
	{
		OnPropertyChanged(nameof(ShowPinAction));
		OnPropertyChanged(nameof(ShowUnpinAction));
	}
}