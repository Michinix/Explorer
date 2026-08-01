using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Explorer.Models;

namespace Explorer.Services;

public sealed class SettingsService
{
	private const int MaxRecentFiles = 15;

	private static readonly string SettingsPath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Explorer", "settings.json");

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		WriteIndented = true
	};

	public SettingsService()
	{
		var settings = Load();
		PinnedItems = new ObservableCollection<PinnedItem>(settings.PinnedItems);
		RecentFiles = new ObservableCollection<RecentFile>(settings.RecentFiles);
		IsGridView = settings.IsGridView;
		IsDetailsPaneVisible = settings.IsDetailsPaneVisible;
	}

	public ObservableCollection<PinnedItem> PinnedItems { get; }
	public ObservableCollection<RecentFile> RecentFiles { get; }
	public bool IsGridView { get; private set; }
	public bool IsDetailsPaneVisible { get; private set; }

	public bool IsPinned(string path)
	{
		return PinnedItems.Any(p => string.Equals(p.FullPath, path, StringComparison.OrdinalIgnoreCase));
	}

	public void TogglePinned(string displayName, string path)
	{
		var existing = PinnedItems.FirstOrDefault(p =>
			string.Equals(p.FullPath, path, StringComparison.OrdinalIgnoreCase));

		if (existing is not null)
			PinnedItems.Remove(existing);
		else
			PinnedItems.Add(new PinnedItem(displayName, path, "/Assets/Icons/Directory.svg", "path { fill: #00AAFF }"));

		Save();
	}

	public void RemovePinned(string path)
	{
		var existing = PinnedItems.FirstOrDefault(p =>
			string.Equals(p.FullPath, path, StringComparison.OrdinalIgnoreCase));

		if (existing is null) return;

		PinnedItems.Remove(existing);
		Save();
	}

	public void SetGridView(bool isGridView)
	{
		IsGridView = isGridView;
		Save();
	}

	public void SetDetailsPaneVisible(bool isVisible)
	{
		IsDetailsPaneVisible = isVisible;
		Save();
	}

	public void AddRecent(string displayName, string path)
	{
		var existing = RecentFiles.FirstOrDefault(r =>
			string.Equals(r.FullPath, path, StringComparison.OrdinalIgnoreCase));

		if (existing is not null)
			RecentFiles.Remove(existing);

		RecentFiles.Insert(0, new RecentFile(displayName, path, DateTime.Now));

		while (RecentFiles.Count > MaxRecentFiles)
			RecentFiles.RemoveAt(RecentFiles.Count - 1);

		Save();
	}

	private static AppSettings Load()
	{
		try
		{
			if (!File.Exists(SettingsPath))
				return BuildDefaults();

			var json = File.ReadAllText(SettingsPath);
			return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? BuildDefaults();
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
			return BuildDefaults();
		}
	}

	private static AppSettings BuildDefaults()
	{
		var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

		return new AppSettings
		{
			PinnedItems =
			[
				new PinnedItem("Bureau", Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
					"/Assets/Icons/Desktop.svg", "path { stroke: white }"),
				new PinnedItem("Téléchargements", Path.Combine(userProfile, "Downloads"),
					"/Assets/Icons/Download.svg", "path { stroke: white }"),
				new PinnedItem("Documents", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
					"/Assets/Icons/Document.svg", "path { stroke: white }"),
				new PinnedItem("Images", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
					"/Assets/Icons/Picture.svg", "path { stroke: white }")
			]
		};
	}

	private void Save()
	{
		try
		{
			var directory = Path.GetDirectoryName(SettingsPath)!;
			Directory.CreateDirectory(directory);

			var settings = new AppSettings
			{
				PinnedItems = [.. PinnedItems],
				RecentFiles = [.. RecentFiles],
				IsGridView = IsGridView,
				IsDetailsPaneVisible = IsDetailsPaneVisible
			};

			File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, JsonOptions));
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
		}
	}
}