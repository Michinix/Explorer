namespace Explorer.Models;

public sealed record OcrCacheEntry(string FullPath, long Length, long LastWriteTicks, string Text);
