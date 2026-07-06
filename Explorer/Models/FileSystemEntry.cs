using System;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Explorer.Services;

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
    [ObservableProperty] private string _editableName = name;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private bool _isNew;
    [ObservableProperty] private bool _isSelected;
    public string Name { get; } = name;
    public string FullPath { get; } = fullPath;
    public string Type { get; } = type;
    public bool IsDirectory { get; } = isDirectory;
    public string DisplaySize { get; } = displaySize;
    public DateTime LastModified { get; } = lastModified;

    public string DisplayType => IsDirectory ? "DOSSIER" : $"Fichier {Type}";
    public string TypeLabel => IsDirectory ? "DOSSIER" : Type;

    public IBrush IconBrush => FileTypeColorsService.GetBrush(Type);
    public string IconLabel => string.IsNullOrEmpty(Type) ? "?" : Type.Length > 3 ? Type[..3] : Type;
}