using System;

namespace Explorer.Models;

public record FileSystemEntry(
    string Name,
    string FullPath,
    string Type,
    bool IsDirectory,
    string DisplaySize,
    DateTime LastModified
)
{
    public string DisplayType => IsDirectory ? "Dossier" : $"Fichier {Type}"; // Propriété calculée 
}