using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Explorer.Services;

public class BrowserService
{
    public static async Task<DirectoryInfo[]?> ListDirectories(string path)
    {
        try {
            return await Task.Run(() =>
                new DirectoryInfo(path)
                    .GetDirectories()
                    .Where(d => !d.Name.StartsWith('.') && !d.Attributes.HasFlag(FileAttributes.Hidden))
                    .OrderBy(d => d.Name)
                    .ToArray()
            );
        } catch {
            return null;
        }
    }

    public static async Task<FileInfo[]?> ListFiles(string path)
    {
        try {
            return await Task.Run(() =>
                new DirectoryInfo(path)
                    .GetFiles()
                    .Where(f => !f.Name.StartsWith('.') && !f.Attributes.HasFlag(FileAttributes.Hidden))
                    .OrderBy(f => f.Name)
                    .ToArray()
            );
        } catch {
            return null;
        }
    }

    public static bool IsValidPath(string path)
    {
        try {
            return Directory.Exists(path);
        } catch {
            return false;
        }
    }
}