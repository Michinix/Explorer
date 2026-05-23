using System;

namespace Explorer.Models;

public class Browser
{
    public string CurrentUser { get; } = Environment.UserName;
    
    public string InitialDirectory { get; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    
    public string CurrentDirectory { get; set; } = Environment.CurrentDirectory;
}