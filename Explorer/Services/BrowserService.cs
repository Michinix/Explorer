using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Explorer.Models;

namespace Explorer.Services;

public class BrowserService(Browser browser)
{
    public async Task<DirectoryInfo[]?> ListDirectories()
    {
        try {
            return await Task.Run(() =>
                new DirectoryInfo(browser.InitialDirectory)
                    .GetDirectories()
                    .Where(d => !d.Name.StartsWith('.') && !d.Attributes.HasFlag(FileAttributes.Hidden))
                    .ToArray()
            );
        } catch {
            return null;
        }
    }

    public async Task<FileInfo[]?> ListFiles()
    {
        try {
            return await Task.Run(() =>
                new DirectoryInfo(browser.InitialDirectory)
                    .GetFiles()
                    .Where(f => !f.Name.StartsWith('.') && !f.Attributes.HasFlag(FileAttributes.Hidden))
                    .ToArray()
            );
        } catch {
            return null;
        }
    }
}