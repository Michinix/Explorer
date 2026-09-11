using System.Collections.Generic;

namespace Explorer.Models;

public record MoveRequestedMessage(IReadOnlyList<FileSystemEntry> Entries, string SourcePath);
