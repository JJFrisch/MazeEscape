using System.Text.Json;
using MazeEscape.Core.Persistence;

namespace MazeEscape.Api.Services;

public sealed class FileSaveStore
{
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly string _storagePath;
    private Dictionary<string, SaveDocument> _documents = new(StringComparer.OrdinalIgnoreCase);

    public FileSaveStore(IHostEnvironment environment)
    {
        var dataFolder = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataFolder);
        _storagePath = Path.Combine(dataFolder, "saves.json");

        if (!File.Exists(_storagePath))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(_storagePath);
            var loaded = JsonSerializer.Deserialize<Dictionary<string, SaveDocument>>(json, _jsonOptions);
            if (loaded != null)
            {
                _documents = loaded;
            }
        }
        catch
        {
            _documents = new Dictionary<string, SaveDocument>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public async Task<SaveDocument?> GetAsync(string playerId, CancellationToken cancellationToken = default)
    {
        await _mutex.WaitAsync(cancellationToken);
        try
        {
            return _documents.TryGetValue(playerId, out var document) ? document : null;
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<SaveDocument> UpsertWithOptimisticConcurrencyAsync(
        string playerId,
        SaveDocument incoming,
        CancellationToken cancellationToken = default)
    {
        await _mutex.WaitAsync(cancellationToken);
        try
        {
            if (_documents.TryGetValue(playerId, out var existing))
            {
                if (string.IsNullOrWhiteSpace(incoming.ConcurrencyToken) || !string.Equals(incoming.ConcurrencyToken, existing.ConcurrencyToken, StringComparison.Ordinal))
                {
                    throw new ConcurrencyConflictException(existing);
                }
            }

            var stored = new SaveDocument
            {
                PlayerId = playerId,
                Version = incoming.Version,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
                PayloadJson = incoming.PayloadJson,
                ConcurrencyToken = Guid.NewGuid().ToString("N")
            };

            _documents[playerId] = stored;
            await PersistAsync(cancellationToken);
            return stored;
        }
        finally
        {
            _mutex.Release();
        }
    }

    private async Task PersistAsync(CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(_documents, _jsonOptions);
        await File.WriteAllTextAsync(_storagePath, json, cancellationToken);
    }
}

public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(SaveDocument existing)
        : base("Concurrency conflict detected.")
    {
        Existing = existing;
    }

    public SaveDocument Existing { get; }
}
