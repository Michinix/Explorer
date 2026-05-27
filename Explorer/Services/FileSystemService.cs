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
    public static bool IsValidPath(string path)
    {
        return Directory.Exists(path);
    }
    
    public static async Task<List<FileSystemEntry>> ListEntries(string path)
    {
        return await Task.Run(() =>
        {
            try
            {
                return new DirectoryInfo(path)
                    .EnumerateFileSystemInfos()
                    .Where(e => {
                        try { return !e.Name.StartsWith('.') && !e.Attributes.HasFlag(FileAttributes.Hidden); }
                        catch (UnauthorizedAccessException) { return false; }
                    })
                    .OrderBy(e => e is FileInfo)
                    .ThenBy(e => e.Name)
                    .Select(e => new FileSystemEntry(
                        Name:         e.Name,
                        Type:         e is DirectoryInfo ? "Dossier" : (e as FileInfo)!.Extension.TrimStart('.').ToUpper(),
                        DisplaySize:  e is FileInfo f ? FormatSize(f.Length) : "—",
                        LastModified: e.LastWriteTime,
                        ItemCount: e is DirectoryInfo d 
                            ? d.EnumerateFileSystemInfos()
                                .Count(x => !x.Name.StartsWith('.') && !x.Attributes.HasFlag(FileAttributes.Hidden))
                                .ToString() 
                            : "—"
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