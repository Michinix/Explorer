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
    public string Name { get; init; } = name;
    public string FullPath { get; init; } = fullPath;
    public string Type { get; init; } = type;
    public bool IsDirectory { get; init; } = isDirectory;
    public string DisplaySize { get; init; } = displaySize;
    public DateTime LastModified { get; init; } = lastModified;

    public string DisplayType => IsDirectory ? "DOSSIER" : $"Fichier {Type}";

    [ObservableProperty] private bool _isSelected;
}