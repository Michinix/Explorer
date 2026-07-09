using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Explorer.Models;

namespace Explorer.Services;

public static class FileSystemService
{
    private static string FormatSize(long bytes)
    {
        const long kb = 1024;
        const long mb = kb * 1024;
        const long gb = mb * 1024;
        const long tb = gb * 1024;

        return bytes switch
        {
            < mb => $"{bytes / (double)kb:0.#} Ko",
            < gb => $"{bytes / (double)mb:0.#} Mo",
            < tb => $"{bytes / (double)gb:0.#} Go",
            _ => $"{bytes / (double)tb:0.#} To"
        };
    }

    public static async Task<ICollection<FileSystemEntry>> ListEntriesAsync(string path)
    {
        return await Task.Run(() =>
            new DirectoryInfo(path)
                .EnumerateFileSystemInfos()
                .Where(e => !e.Name.StartsWith('.') && !e.Attributes.HasFlag(FileAttributes.Hidden))
                .OrderBy(e => e is FileInfo)
                .ThenBy(e => e.Name)
                .Select(e => new FileSystemEntry(
                    e.Name,
                    e.FullName,
                    e.Extension.TrimStart('.').ToUpper(),
                    e is DirectoryInfo,
                    e is FileInfo f ? FormatSize(f.Length) : "—",
                    e.LastWriteTime
                ))
                .ToArray());
    }

    public static async Task LaunchFileAsync(string path)
    {
        await Task.Run(() =>
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }));
    }

    public static async Task<ICollection<DriveItem>> GetDrivesAsync()
    {
        return await Task.Run(() => DriveInfo.GetDrives()
            .Where(d => d is { IsReady: true, DriveType: DriveType.Removable or DriveType.Fixed }
                        && (!OperatingSystem.IsMacOS() || d.Name is "/" || d.Name.StartsWith("/Volumes/")))
            .Select(d =>
            {
                var label = string.IsNullOrWhiteSpace(d.VolumeLabel) ? d.Name.TrimEnd('\\') : d.VolumeLabel;
                var typeName = d.DriveType == DriveType.Removable
                    ? "Amovible"
                    : d.Name.StartsWith("C:") || d.Name == "/"
                        ? "Système"
                        : "Données";

                return new DriveItem(
                    label,
                    typeName,
                    $"{FormatSize(d.AvailableFreeSpace)} libres",
                    d.Name
                );
            })
            .OrderBy(d => d.Type)
            .ThenBy(d => d.DisplayName)
            .ToArray());
    }

    public static async Task CreateFileAsync(string path)
    {
        await Task.Run(() => new FileStream(path, FileMode.CreateNew).Dispose());
    }

    public static async Task CreateDirectoryAsync(string path)
    {
        await Task.Run(() => Directory.CreateDirectory(path));
    }

    public static async Task RenameAsync(FileSystemEntry entry, string newName)
    {
        var directory = Path.GetDirectoryName(entry.FullPath)!;
        var destination = Path.Combine(directory, newName);

        await Task.Run(() =>
        {
            if (entry.IsDirectory)
                Directory.Move(entry.FullPath, destination);
            else
                File.Move(entry.FullPath, destination);
        });
    }

    public static async Task DeleteEntriesAsync(IEnumerable<FileSystemEntry> entries)
    {
        await Task.Run(() =>
        {
            foreach (var entry in entries)
                if (entry.IsDirectory)
                    Directory.Delete(entry.FullPath, true);
                else
                    File.Delete(entry.FullPath);
        });
    }
}