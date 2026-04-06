using MazeEscape.Core.Persistence;
using MazeEscape.Core.Sync;

namespace MazeEscape.Web.Services;

public sealed class SyncCoordinator
{
    private readonly ISaveRepository _saveRepository;
    private readonly ICloudSyncService _cloudSyncService;
    private readonly SyncStatusService _syncStatus;

    public SyncCoordinator(
        ISaveRepository saveRepository,
        ICloudSyncService cloudSyncService,
        SyncStatusService syncStatus)
    {
        _saveRepository = saveRepository;
        _cloudSyncService = cloudSyncService;
        _syncStatus = syncStatus;
    }

    public async Task SaveLocalAsync(string playerId, string payloadJson, CancellationToken cancellationToken = default)
    {
        var existingLocal = await _saveRepository.LoadAsync(playerId, cancellationToken);
        var document = new SaveDocument
        {
            PlayerId = playerId,
            Version = existingLocal?.Version ?? "1.0",
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            PayloadJson = payloadJson,
            ConcurrencyToken = existingLocal?.ConcurrencyToken
        };

        await _saveRepository.SaveAsync(document, cancellationToken);
        _syncStatus.SetIdle("Saved locally to IndexedDB.");
    }

    public async Task BootstrapFromCloudAsync(string playerId, CancellationToken cancellationToken = default)
    {
        _syncStatus.SetSyncing("Checking cloud save...");

        var cloud = await _cloudSyncService.PullAsync(playerId, cancellationToken);
        if (cloud == null)
        {
            _syncStatus.SetIdle("No cloud save found. Working from local data.");
            return;
        }

        await _saveRepository.SaveAsync(cloud, cancellationToken);
        _syncStatus.SetIdle("Cloud save loaded to local storage.");
    }

    public async Task SyncNowAsync(string playerId, CancellationToken cancellationToken = default)
    {
        _syncStatus.SetSyncing("Syncing to cloud...");

        var local = await _saveRepository.LoadAsync(playerId, cancellationToken);
        if (local == null)
        {
            _syncStatus.SetIdle("No local save found to sync.");
            return;
        }

        var result = await _cloudSyncService.PushAsync(local, cancellationToken);
        if (result.Success)
        {
            var pulled = await _cloudSyncService.PullAsync(playerId, cancellationToken);
            if (pulled != null)
            {
                await _saveRepository.SaveAsync(pulled, cancellationToken);
            }

            _syncStatus.SetIdle(result.Message ?? "Cloud sync completed.");
            return;
        }

        if (result.ConflictDetected)
        {
            var cloud = await _cloudSyncService.PullAsync(playerId, cancellationToken);
            if (cloud != null)
            {
                await _saveRepository.SaveAsync(cloud, cancellationToken);
            }
        }

        _syncStatus.SetIdle(result.Message ?? "Cloud sync failed.");
    }

    public async Task<string> LoadLocalSummaryAsync(string playerId, CancellationToken cancellationToken = default)
    {
        var local = await _saveRepository.LoadAsync(playerId, cancellationToken);
        if (local == null)
        {
            return "No local save found.";
        }

        return $"Found save v{local.Version} updated {local.UpdatedAtUtc:O}";
    }
}
