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
                        Type: e.Extension.TrimStart('.').ToUpper(),
                        IsDirectory: e is DirectoryInfo,
                        DisplaySize: e is FileInfo f ? FormatSize(f.Length) : "—",
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
    
    private static string FormatSize(long bytes)
    {
        const long kb = 1024;
        const long mb = kb * 1024;
        const long gb = mb * 1024;
        const long tb = gb * 1024;

        return bytes switch
        {
            < kb => $"{bytes} o",
            < mb => $"{bytes / (double)kb:0.#} Ko",
            < gb => $"{bytes / (double)mb:0.#} Mo",
            < tb => $"{bytes / (double)gb:0.#} Go",
            _    => $"{bytes / (double)tb:0.#} To"
        };
    }
}