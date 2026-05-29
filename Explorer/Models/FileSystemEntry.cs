using System;

namespace Explorer.Models;

public record FileSystemEntry(
    string Name,
    string FullPath,
    string Type,
    string DisplaySize,
    DateTime LastModified,
    string ItemCount
);