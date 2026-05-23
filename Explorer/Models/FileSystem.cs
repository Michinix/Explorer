using System;

namespace Explorer.Models;

public static class FileSystem
{
    public static string CurrentUser { get; } = Environment.UserName;
    public static string HomeDirectory { get; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
}