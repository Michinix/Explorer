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
        const long KB = 1024;
        const long MB = KB * 1024;
        const long GB = MB * 1024;
        const long TB = GB * 1024;

        return bytes switch
        {
            < KB => $"{bytes} o",
            < MB => $"{bytes / (double)KB:0.#} Ko",
            < GB => $"{bytes / (double)MB:0.#} Mo",
            < TB => $"{bytes / (double)GB:0.#} Go",
            _    => $"{bytes / (double)TB:0.#} To"
        };
    }
}