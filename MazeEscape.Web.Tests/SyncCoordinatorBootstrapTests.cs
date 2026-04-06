using MazeEscape.Core.Persistence;
using MazeEscape.Core.Sync;
using MazeEscape.Web.Services;
using Xunit;

namespace MazeEscape.Web.Tests;

public class SyncCoordinatorBootstrapTests
{
    [Fact]
    public async Task BootstrapFromCloud_LoadsCloudDocumentIntoLocalRepository()
    {
        var cloudDocument = new SaveDocument
        {
            PlayerId = "player-001",
            Version = "1.0",
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            PayloadJson = "{\"coins\":400}",
            ConcurrencyToken = "server-v1"
        };

        var repository = new InMemorySaveRepository();
        var cloudSync = new StubCloudSyncService(cloudDocument);
        var status = new SyncStatusService();
        var coordinator = new SyncCoordinator(repository, cloudSync, status);

        await coordinator.BootstrapFromCloudAsync("player-001");

        var saved = await repository.LoadAsync("player-001");
        Assert.NotNull(saved);
        Assert.Equal("{\"coins\":400}", saved!.PayloadJson);
        Assert.Equal("server-v1", saved.ConcurrencyToken);
        Assert.Equal("Cloud save loaded to local storage.", status.StatusText);
    }

    private sealed class InMemorySaveRepository : ISaveRepository
    {
        private readonly Dictionary<string, SaveDocument> _documents = new(StringComparer.Ordinal);

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

    private sealed class StubCloudSyncService : ICloudSyncService
    {
        private readonly SaveDocument? _cloudDocument;

        public StubCloudSyncService(SaveDocument? cloudDocument)
        {
            _cloudDocument = cloudDocument;
        }

        public Task<SyncResult> PushAsync(SaveDocument document, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new SyncResult
            {
                Success = true,
                ConflictDetected = false,
                Message = "ok"
            });
        }

        public Task<SaveDocument?> PullAsync(string playerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_cloudDocument);
        }
    }
}
