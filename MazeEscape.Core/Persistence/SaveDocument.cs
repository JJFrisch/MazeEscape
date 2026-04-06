namespace MazeEscape.Core.Persistence;

public sealed class SaveDocument
{
    public required string PlayerId { get; init; }
    public required string Version { get; init; }
    public required DateTimeOffset UpdatedAtUtc { get; init; }
    public required string PayloadJson { get; init; }
    public string? ConcurrencyToken { get; init; }
}
