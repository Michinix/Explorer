using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Explorer.Models;

namespace Explorer.Services;

public class BrowserService
{
    private readonly Browser _browser;

    public BrowserService(Browser browser)
    {
        _browser = browser;
    }
    
    public string GetUser() => _browser.CurrentUser;
    
    public async Task<DirectoryInfo[]> ListDirectories()
    {
        try {
            return await Task.Run(() =>
                new DirectoryInfo(_browser.InitialDirectory)
                    .GetDirectories()
                    .Where(d => !d.Name.StartsWith('.') && !d.Attributes.HasFlag(FileAttributes.Hidden))
                    .ToArray()
            );
        } catch (UnauthorizedAccessException) {
            return [];
        }
    }

    public async Task<FileInfo[]> ListFiles()
    {
        try {
            return await Task.Run(() =>
                new DirectoryInfo(_browser.InitialDirectory)
                    .GetFiles()
                    .Where(f => !f.Name.StartsWith('.') && !f.Attributes.HasFlag(FileAttributes.Hidden))
                    .ToArray()
            );
        } catch (UnauthorizedAccessException) {
            return [];
        }
    }
}