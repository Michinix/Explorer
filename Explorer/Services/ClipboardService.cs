using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Explorer.Models;

namespace Explorer.Services;

public enum ClipboardOperation
{
    None,
    Copy,
    Cut
}

public partial class ClipboardService : ObservableObject
{
    private FileSystemEntry[] _entries = [];

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanPaste))]
    private ClipboardOperation _operation = ClipboardOperation.None;

    public bool CanPaste => Operation != ClipboardOperation.None && _entries.Length > 0;

    public void Copy(IEnumerable<FileSystemEntry> entries)
    {
        Stage(entries, ClipboardOperation.Copy);
    }

    public void Cut(IEnumerable<FileSystemEntry> entries)
    {
        Stage(entries, ClipboardOperation.Cut);
    }

    private void Clear()
    {
        _entries = [];
        Operation = ClipboardOperation.None;
    }

    public async Task PasteAsync(string destinationDirectory)
    {
        if (!CanPaste) return;

        var entries = _entries;
        var operation = Operation;

        await Task.Run(() =>
        {
            foreach (var entry in entries)
            {
                var sourceDirectory = Path.GetDirectoryName(entry.FullPath);

                if (operation == ClipboardOperation.Cut &&
                    string.Equals(sourceDirectory, destinationDirectory, StringComparison.OrdinalIgnoreCase))
                    continue;

                var destination = UniqueDestination(destinationDirectory, entry);

                if (operation == ClipboardOperation.Cut)
                    MoveEntry(entry, destination);
                else
                    CopyEntry(entry, destination);
            }
        });

        if (operation == ClipboardOperation.Cut)
            Clear();
    }

    private void Stage(IEnumerable<FileSystemEntry> entries, ClipboardOperation operation)
    {
        _entries = entries.Where(e => !e.IsNew).ToArray();
        Operation = _entries.Length > 0 ? operation : ClipboardOperation.None;
    }

    private static string UniqueDestination(string destinationDirectory, FileSystemEntry entry)
    {
        var destination = Path.Combine(destinationDirectory, entry.Name);
        if (!Exists(destination))
            return destination;

        var name = Path.GetFileNameWithoutExtension(entry.Name);
        var extension = entry.IsDirectory ? string.Empty : Path.GetExtension(entry.Name);
        var stem = entry.IsDirectory ? entry.Name : name;

        for (var i = 1;; i++)
        {
            var candidate = Path.Combine(destinationDirectory, $"{stem} ({i}){extension}");
            if (!Exists(candidate))
                return candidate;
        }
    }

    private static void CopyEntry(FileSystemEntry entry, string destination)
    {
        if (entry.IsDirectory)
            CopyDirectory(entry.FullPath, destination);
        else
            File.Copy(entry.FullPath, destination);
    }

    private static void MoveEntry(FileSystemEntry entry, string destination)
    {
        if (entry.IsDirectory)
            Directory.Move(entry.FullPath, destination);
        else
            File.Move(entry.FullPath, destination);
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);

        foreach (var file in Directory.EnumerateFiles(source))
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)));

        foreach (var directory in Directory.EnumerateDirectories(source))
            CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)));
    }

    private static bool Exists(string path)
    {
        return File.Exists(path) || Directory.Exists(path);
    }
}