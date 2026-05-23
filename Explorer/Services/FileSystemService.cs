using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Explorer.Services;

public static class FileSystemService
{
    public static bool IsValidPath(string path)
    {
        try {
            return Directory.Exists(path);
        } catch {
            return false;
        }
    }

    public static async Task<DirectoryInfo[]?> ListDirectories(string path)
    {
        try {
            return await Task.Run(() =>
                new DirectoryInfo(path)
                    .GetDirectories()
                    .Where(d => !d.Name.StartsWith('.') && !d.Attributes.HasFlag(FileAttributes.Hidden))
                    .ToArray()
            );
        } catch (Exception ex) {
            Debug.WriteLine(ex.Message);
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
                    .ToArray()
            );
        } catch (Exception ex) {
            Debug.WriteLine(ex.Message);
            return null;
        }
    }
}