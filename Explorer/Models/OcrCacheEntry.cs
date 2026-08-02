namespace Explorer.Models;

public sealed record OcrCacheEntry(long Length, long LastWriteTicks, string Text);