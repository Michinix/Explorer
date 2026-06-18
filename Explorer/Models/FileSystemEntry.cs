using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Explorer.Models;

public partial class FileSystemEntry(
    string name,
    string fullPath,
    string type,
    bool isDirectory,
    string displaySize,
    DateTime lastModified
) : ObservableObject
{
    public string Name { get; } = name;
    public string FullPath { get; } = fullPath;
    public string Type { get; } = type;
    public bool IsDirectory { get; } = isDirectory;
    public string DisplaySize { get; } = displaySize;
    public DateTime LastModified { get; } = lastModified;

    public string DisplayType => IsDirectory ? "DOSSIER" : $"Fichier {Type}";

    [ObservableProperty] private bool _isSelected;
}