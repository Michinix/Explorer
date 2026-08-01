using System;

namespace Explorer.Models;

public sealed record RecentFile(string DisplayName, string FullPath, DateTime OpenedAt);