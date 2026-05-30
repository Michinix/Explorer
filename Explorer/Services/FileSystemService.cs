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
    public static async Task<List<FileSystemEntry>> ListEntries(string path)
    {
        return await Task.Run(() =>
        {
            try
            {
                return new DirectoryInfo(path)
                    .EnumerateFileSystemInfos()
                    .Where(e => !e.Name.StartsWith('.') && !e.Attributes.HasFlag(FileAttributes.Hidden))
                    .OrderBy(e => e is FileInfo)
                    .ThenBy(e => e.Name)
                    .Select(e => new FileSystemEntry(
                        Name: e.Name,
                        FullPath: e.FullName,
                        Type: e is DirectoryInfo ? "DOSSIER" : e.Extension.TrimStart('.').ToUpper(),
                        IsDirectory: e is DirectoryInfo,
                        DisplaySize:  e is FileInfo f ? FormatSize(f.Length) : "—",
                        LastModified: e.LastWriteTime
                    ))
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return [];
            }
        });
    }

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024        => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        _             => $"{bytes / (1024.0 * 1024):F1} MB"
    };
}