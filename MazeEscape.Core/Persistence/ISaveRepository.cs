namespace MazeEscape.Core.Persistence;

public interface ISaveRepository
{
    Task<SaveDocument?> LoadAsync(string playerId, CancellationToken cancellationToken = default);
    Task SaveAsync(SaveDocument document, CancellationToken cancellationToken = default);
}
