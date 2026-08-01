using System.Collections.Generic;

namespace Explorer.Models;

public sealed class AppSettings
{
	public List<PinnedItem> PinnedItems { get; set; } = [];
	public List<RecentFile> RecentFiles { get; set; } = [];
	public bool IsGridView { get; set; }
	public bool IsDetailsPaneVisible { get; set; } = true;
}