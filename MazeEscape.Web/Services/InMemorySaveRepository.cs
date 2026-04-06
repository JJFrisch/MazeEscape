using MazeEscape.Core.Persistence;

namespace MazeEscape.Web.Services;

public sealed class InMemorySaveRepository : ISaveRepository
{
    private readonly Dictionary<string, SaveDocument> _documents = new();

    public Task<SaveDocument?> LoadAsync(string playerId, CancellationToken cancellationToken = default)
    {
        _documents.TryGetValue(playerId, out var document);
        return Task.FromResult(document);
    }

    public Task SaveAsync(SaveDocument document, CancellationToken cancellationToken = default)
    {
        _documents[document.PlayerId] = document;
        return Task.CompletedTask;
    }
}
