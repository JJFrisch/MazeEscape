using MazeEscape.Models;

namespace MazeEscape.Services
{
    /// <summary>
    /// Implements checkpoint save synchronization with conflict handling and offline queue.
    /// </summary>
    public class SaveSynchronizer : ISaveSynchronizer
    {
        private readonly IApiClient _apiClient;
        private readonly IGameInitializer _gameInitializer;
        private readonly IOfflineStorageService _offlineStorage;
        private SaveDocument? _currentSaveDocument;

        public SaveSynchronizer(
            IApiClient apiClient,
            IGameInitializer gameInitializer,
            IOfflineStorageService offlineStorage)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _gameInitializer = gameInitializer ?? throw new ArgumentNullException(nameof(gameInitializer));
            _offlineStorage = offlineStorage ?? throw new ArgumentNullException(nameof(offlineStorage));
        }

        public async Task SaveCheckpointAsync(PlayerDataModel playerData, string checkpointName)
        {
            ArgumentNullException.ThrowIfNull(playerData);
            if (string.IsNullOrEmpty(checkpointName))
                checkpointName = "checkpoint";

            try
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[SaveSynchronizer] Saving checkpoint: {checkpointName} for player {playerData.PlayerId}"
                );

                // Serialize game state to payload
                var payload = GameStateMapper.ToPayload(playerData);
                var payloadJson = GameStateMapper.SerializePayload(payload);

                // Build save document with current concurrency token (or null if first save)
                var concurrencyToken = _currentSaveDocument?.ConcurrencyToken;
                var saveDoc = new SaveDocument
                {
                    PlayerId = playerData.PlayerId.ToString(),
                    Version = "1.0",
                    PayloadJson = payloadJson,
                    UpdatedAtUtc = DateTimeOffset.UtcNow,
                    ConcurrencyToken = concurrencyToken
                };

                // Try to save with exponential backoff and conflict resolution
                await SaveWithRetryAsync(playerData.PlayerId.ToString(), saveDoc, checkpointName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[SaveSynchronizer] Checkpoint save failed (will queue offline): {ex.Message}"
                );

                // Queue to offline storage for later sync
                try
                {
                    var payload = GameStateMapper.ToPayload(playerData);
                    await _offlineStorage.EnqueueSaveAsync(
                        payload,
                        _currentSaveDocument?.ConcurrencyToken,
                        checkpointName
                    );
                }
                catch (Exception queueEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[SaveSynchronizer] Failed to queue offline save: {queueEx.Message}");
                }
            }
        }

        public async Task ProcessOfflineQueueAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[SaveSynchronizer] Processing offline queue...");

                var queuedSaves = await _offlineStorage.GetQueueAsync();
                if (!queuedSaves.Any())
                {
                    System.Diagnostics.Debug.WriteLine("[SaveSynchronizer] Offline queue is empty.");
                    return;
                }

                int successCount = 0;
                foreach (var queuedSave in queuedSaves)
                {
                    try
                    {
                        // Rebuild SaveDocument from queued data
                        var saveDoc = new SaveDocument
                        {
                            PlayerId = queuedSave.PlayerId,
                            Version = "1.0",
                            PayloadJson = GameStateMapper.SerializePayload(queuedSave.Payload),
                            UpdatedAtUtc = queuedSave.SaveTimeUtc,
                            ConcurrencyToken = queuedSave.ConcurrencyToken
                        };

                        // Try to save
                        var result = await _apiClient.PutSaveAsync(queuedSave.PlayerId, saveDoc);
                        _currentSaveDocument = result;

                        // Remove from queue on success
                        await _offlineStorage.RemoveFromQueueAsync(queuedSave.QueueId);
                        successCount++;

                        System.Diagnostics.Debug.WriteLine(
                            $"[SaveSynchronizer] Successfully synced queued save: {queuedSave.CheckpointName}"
                        );
                    }
                    catch (ConflictException confEx)
                    {
                        // Conflict: use server version and remove from queue
                        // (Could implement smarter merge here in v2)
                        System.Diagnostics.Debug.WriteLine(
                            $"[SaveSynchronizer] Conflict on queued save {queuedSave.CheckpointName}. Using server version."
                        );
                        _currentSaveDocument = confEx.ServerSaveDocument;
                        await _offlineStorage.RemoveFromQueueAsync(queuedSave.QueueId);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[SaveSynchronizer] Failed to sync queued save {queuedSave.CheckpointName}: {ex.Message}"
                        );
                        // Keep in queue for retry on next resume
                    }
                }

                System.Diagnostics.Debug.WriteLine(
                    $"[SaveSynchronizer] Offline queue processing complete. Synced {successCount}/{queuedSaves.Count} saves."
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SaveSynchronizer] Offline queue processing failed: {ex.Message}");
            }
        }

        public async Task<int> GetOfflineQueueCountAsync()
        {
            try
            {
                var queuedSaves = await _offlineStorage.GetQueueAsync();
                return queuedSaves.Count;
            }
            catch
            {
                return 0;
            }
        }

        private async Task SaveWithRetryAsync(string playerId, SaveDocument saveDoc, string checkpointName)
        {
            const int maxRetries = 3;
            int[] retryDelaysMs = { 500, 1000, 2000 };

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    var result = await _apiClient.PutSaveAsync(playerId, saveDoc);
                    _currentSaveDocument = result;

                    System.Diagnostics.Debug.WriteLine(
                        $"[SaveSynchronizer] Checkpoint '{checkpointName}' saved successfully."
                    );
                    return; // Success
                }
                catch (ConflictException confEx)
                {
                    // Conflict: retry with server's new token
                    System.Diagnostics.Debug.WriteLine(
                        $"[SaveSynchronizer] Conflict on attempt {attempt + 1}/{maxRetries}. " +
                        $"Retrying with server token..."
                    );

                    _currentSaveDocument = confEx.ServerSaveDocument;
                    saveDoc.ConcurrencyToken = confEx.ServerSaveDocument.ConcurrencyToken;

                    if (attempt < maxRetries - 1)
                    {
                        await Task.Delay(retryDelaysMs[attempt]);
                        // Retry loop will continue
                    }
                    else
                    {
                        throw; // Final retry exhausted
                    }
                }
                catch (Exception ex)
                {
                    // Network or other error
                    System.Diagnostics.Debug.WriteLine(
                        $"[SaveSynchronizer] Save attempt {attempt + 1}/{maxRetries} failed: {ex.Message}"
                    );

                    if (attempt < maxRetries - 1)
                    {
                        await Task.Delay(retryDelaysMs[attempt]);
                        // Retry loop will continue
                    }
                    else
                    {
                        throw; // Final retry exhausted
                    }
                }
            }
        }
    }
}
