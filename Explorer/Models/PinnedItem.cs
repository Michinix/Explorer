using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Explorer.Models;

public partial class PinnedItem(string displayName, string fullPath, string iconPath, string iconCss) : ObservableObject
{
	[property: JsonIgnore] [ObservableProperty]
	private bool _isActive;

	public string DisplayName { get; } = displayName;
	public string FullPath { get; } = fullPath;
	public string IconPath { get; } = iconPath;
	public string IconCss { get; } = iconCss;
}