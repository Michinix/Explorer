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
    public static async Task<ICollection<FileSystemEntry>> ListEntries(string path)
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
                        e.Name,
                        e.FullName,
                        e.Extension.TrimStart('.').ToUpper(),
                        e is DirectoryInfo,
                        e is FileInfo f ? FormatSize(f.Length) : "—",
                        e.LastWriteTime
                    ))
                    .ToArray();
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
            < mb => $"{bytes / (double)kb:0.#} Ko",
            < gb => $"{bytes / (double)mb:0.#} Mo",
            < tb => $"{bytes / (double)gb:0.#} Go",
            _    => $"{bytes / (double)tb:0.#} To"
        };
    }
}