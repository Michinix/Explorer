using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Explorer.Services;

public static class FileSystemService
{
    public static bool IsValidPath(string path)
    {
        return Directory.Exists(path);
    }
    
    public static async Task<List<FileSystemInfo>> ListEntries(string path)
    {
        return await Task.Run(() =>
        {
            try
            {
                return new DirectoryInfo(path)
                    .EnumerateFileSystemInfos()
                    .Where(e =>
                    {
                        try
                        {
                            return !e.Name.StartsWith('.')
                                   && !e.Attributes.HasFlag(FileAttributes.Hidden);
                        }
                        catch (UnauthorizedAccessException) { return false; }
                    })
                    .OrderBy(e => e is FileInfo)
                    .ThenBy(e => e.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return [];
            }
        });
    }
}