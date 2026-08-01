using CommunityToolkit.Mvvm.ComponentModel;

namespace Explorer.Models;

public partial class DriveItem(
	string displayName,
	string type,
	string fullPath,
	long totalSize = 0,
	long freeSpace = 0,
	string sizeText = "") : ObservableObject
{
	[ObservableProperty] private bool _isActive;

	public string DisplayName { get; } = displayName;
	public string Type { get; } = type;
	public string FullPath { get; } = fullPath;
	public string SizeText { get; } = sizeText;
	public double UsedPercent { get; } = totalSize > 0 ? (totalSize - freeSpace) / (double)totalSize * 100d : 0d;
}