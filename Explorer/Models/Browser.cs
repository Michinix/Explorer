using System;

namespace Explorer.Models;

public class Browser
{
    public string CurrentUser { get; } = Environment.UserName;
    public string HomeDirectory { get; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
}