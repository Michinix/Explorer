using CommunityToolkit.Mvvm.ComponentModel;

namespace Explorer.Models;

public partial class DriveItem(string displayName, string type, string fullPath) : ObservableObject
{
    [ObservableProperty] private bool _isActive;

    public string DisplayName { get; } = displayName;
    public string Type { get; } = type;
    public string FullPath { get; } = fullPath;
}