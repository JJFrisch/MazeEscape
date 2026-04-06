using MazeEscape.Core.Persistence;

namespace MazeEscape.Core.Sync;

public interface ICloudSyncService
{
    Task<SyncResult> PushAsync(SaveDocument document, CancellationToken cancellationToken = default);
    Task<SaveDocument?> PullAsync(string playerId, CancellationToken cancellationToken = default);
}
