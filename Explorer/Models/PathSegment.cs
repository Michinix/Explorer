namespace Explorer.Models;

public sealed record PathSegment(string Name, string FullPath, bool IsFirst, bool IsHome = false)
{
	public bool IsDriveRoot => IsFirst && !IsHome;
}