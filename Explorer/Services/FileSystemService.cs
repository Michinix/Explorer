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
	private static string FormatSize(long bytes)
	{
		const long kb = 1024;
		const long mb = kb * 1024;
		const long gb = mb * 1024;
		const long tb = gb * 1024;

		return bytes switch
		{
			< mb => $"{bytes / (double)kb:0.##} Ko",
			< gb => $"{bytes / (double)mb:0.##} Mo",
			< tb => $"{bytes / (double)gb:0.##} Go",
			_ => $"{bytes / (double)tb:0.##} To"
		};
	}

	private static EnumerationOptions CreateRecursiveOptions()
	{
		return new EnumerationOptions
		{
			RecurseSubdirectories = true,
			IgnoreInaccessible = true,
			AttributesToSkip = FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReparsePoint
		};
	}

	public static Task<FileSystemEntry[]> SearchEntriesAsync(string path, string searchTerm)
	{
		return Task.Run(() =>
		{
			return new DirectoryInfo(path)
				.EnumerateFileSystemInfos("*", CreateRecursiveOptions())
				.Where(e => e.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
				.Select(MapToEntry)
				.DistinctBy(e => e.FullPath, StringComparer.OrdinalIgnoreCase)
				.ToArray();
		});
	}

	public static Task<FileSystemEntry[]> ListImagesRecursiveAsync(string path)
	{
		return Task.Run(() =>
		{
			return new DirectoryInfo(path)
				.EnumerateFiles("*", CreateRecursiveOptions())
				.Select(MapToEntry)
				.Where(e => e.IsImage)
				.DistinctBy(e => e.FullPath, StringComparer.OrdinalIgnoreCase)
				.OrderBy(e => e.FullPath, StringComparer.OrdinalIgnoreCase)
				.ToArray();
		});
	}

	public static Task<FileSystemEntry[]> ListEntriesAsync(string path)
	{
		return Task.Run(() =>
		{
			var options = new EnumerationOptions
			{
				IgnoreInaccessible = true,
				AttributesToSkip = FileAttributes.None
			};

			return new DirectoryInfo(path)
				.EnumerateFileSystemInfos("*", options)
				.Where(e => e.Name.StartsWith('.') ||
				            (!e.Attributes.HasFlag(FileAttributes.Hidden) &&
				             !e.Attributes.HasFlag(FileAttributes.System)))
				.Where(e => !string.Equals(e.Name, "Photos Library.jpeg", StringComparison.OrdinalIgnoreCase))
				.OrderBy(e => e is FileInfo)
				.ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
				.Select(MapToEntry)
				.ToArray();
		});
	}

	public static Task LaunchFileAsync(string path)
	{
		return Task.Run(() => Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }));
	}

	public static Task<DriveItem[]> GetDrivesAsync()
	{
		return Task.Run(() =>
		{
			var drives = DriveInfo.GetDrives();
			var result = new List<DriveItem>(drives.Length);

			foreach (var drive in drives)
				try
				{
					if (!drive.IsReady) continue;
					if (drive.DriveType is not (DriveType.Removable or DriveType.Fixed)) continue;
					if (OperatingSystem.IsMacOS() && drive.Name != "/" && !drive.Name.StartsWith("/Volumes/")) continue;

					var isSystem = drive.Name.StartsWith("C:", StringComparison.OrdinalIgnoreCase) || drive.Name == "/";
					var rawLetter = drive.Name.TrimEnd('\\', '/');

					var label = !string.IsNullOrWhiteSpace(drive.VolumeLabel)
						? drive.VolumeLabel
						: isSystem
							? "Disque Système"
							: "Disque Local";

					var displayName = $"{label} ({rawLetter})";
					var type = drive.DriveType == DriveType.Removable ? "Amovible" : isSystem ? "Système" : "Données";

					var totalSize = drive.TotalSize;
					var freeSpace = drive.AvailableFreeSpace;
					var sizeText = $"{FormatSize(freeSpace)} libre(s) sur {FormatSize(totalSize)}";

					result.Add(new DriveItem(displayName, type, drive.Name, totalSize, freeSpace, sizeText));
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}

			return result
				.OrderBy(item => item.Type == "Système" ? 0 : 1)
				.ThenBy(item => item.Type)
				.ToArray();
		});
	}

	public static Task<DriveItem[]> GetCloudStorageAsync()
	{
		return Task.Run(() =>
		{
			var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			var result = new List<DriveItem>();

			TryAdd("OneDrive", Environment.GetEnvironmentVariable("OneDriveConsumer"));
			TryAdd("OneDrive", Environment.GetEnvironmentVariable("OneDrive"));
			TryAdd("OneDrive Entreprise", Environment.GetEnvironmentVariable("OneDriveCommercial"));
			TryAdd("iCloud Drive", Path.Combine(userProfile, "iCloudDrive"));
			TryAdd("iCloud Photos",
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "iCloud Photos",
					"Photos"));
			TryAdd("Google Drive", Path.Combine(userProfile, "Google Drive"));
			TryAdd("Dropbox", Path.Combine(userProfile, "Dropbox"));

			return result.ToArray();

			void TryAdd(string displayName, string? path)
			{
				try
				{
					if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path) &&
					    result.All(item => !string.Equals(item.FullPath, path, StringComparison.OrdinalIgnoreCase)))
						result.Add(new DriveItem(displayName, "Cloud", path));
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
			}
		});
	}

	public static Task CreateFileAsync(string path)
	{
		return Task.Run(() => File.Create(path).Dispose());
	}

	public static Task CreateDirectoryAsync(string path)
	{
		return Task.Run(() => Directory.CreateDirectory(path));
	}

	public static Task RenameAsync(FileSystemEntry entry, string newName)
	{
		var directory = Path.GetDirectoryName(entry.FullPath)!;
		var destination = Path.Combine(directory, newName);

		return Task.Run(() =>
		{
			if (entry.IsDirectory)
				Directory.Move(entry.FullPath, destination);
			else
				File.Move(entry.FullPath, destination);
		});
	}

	public static Task DeleteEntriesAsync(IEnumerable<FileSystemEntry> entries)
	{
		return Task.Run(() =>
		{
			foreach (var entry in entries)
				try
				{
					if (entry.IsDirectory)
						Directory.Delete(entry.FullPath, true);
					else
						File.Delete(entry.FullPath);
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
		});
	}

	private static FileSystemEntry MapToEntry(FileSystemInfo e)
	{
		var ext = e.Extension.Length > 0 ? e.Extension[1..].ToUpperInvariant() : string.Empty;
		var size = e is FileInfo f ? FormatSize(f.Length) : "—";
		return new FileSystemEntry(e.Name, e.FullName, ext, e is DirectoryInfo, size, e.LastWriteTime,
			e.LastAccessTime);
	}
}