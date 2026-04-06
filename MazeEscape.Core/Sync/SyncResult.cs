namespace MazeEscape.Core.Sync;

public sealed class SyncResult
{
    public required bool Success { get; init; }
    public required bool ConflictDetected { get; init; }
    public string? Message { get; init; }
}
