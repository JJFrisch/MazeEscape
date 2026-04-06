using MazeEscape.Models;
using System.Text.Json;

namespace MazeEscape.Services
{
    /// <summary>
    /// Implements offline save queue using JSON files in AppDataDirectory.
    /// </summary>
    public class OfflineStorageService : IOfflineStorageService
    {
        private const string QUEUE_FILENAME = "offline_saves_queue.json";
        private readonly string _queueFilePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public OfflineStorageService()
        {
            _queueFilePath = Path.Combine(FileSystem.AppDataDirectory, QUEUE_FILENAME);
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        public async Task EnqueueSaveAsync(SaveGamePayload payload, string? concurrencyToken, string checkpointName)
        {
            ArgumentNullException.ThrowIfNull(payload);

            try
            {
                var queueId = Guid.NewGuid().ToString();
                var queuedSave = new QueuedSave
                {
                    QueueId = queueId,
                    PlayerId = payload.PlayerId,
                    Payload = payload,
                    ConcurrencyToken = concurrencyToken,
                    SaveTimeUtc = DateTimeOffset.UtcNow,
                    CheckpointName = checkpointName ?? "unknown",
                    RetryCount = 0
                };

                var queue = await GetQueueAsync();
                queue.Add(queuedSave);

                await WriteQueueAsync(queue);

                System.Diagnostics.Debug.WriteLine(
                    $"[OfflineStorageService] Queued save: {checkpointName} (ID: {queueId})"
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[OfflineStorageService] Failed to enqueue save: {ex.Message}"
                );
                throw;
            }
        }

        public async Task<List<QueuedSave>> GetQueueAsync()
        {
            try
            {
                if (!File.Exists(_queueFilePath))
                {
                    return new List<QueuedSave>();
                }

                var json = await File.ReadAllTextAsync(_queueFilePath);
                if (string.IsNullOrEmpty(json))
                {
                    return new List<QueuedSave>();
                }

                var queue = JsonSerializer.Deserialize<List<QueuedSave>>(json, _jsonOptions);
                return queue ?? new List<QueuedSave>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[OfflineStorageService] Failed to read queue: {ex.Message}"
                );
                return new List<QueuedSave>();
            }
        }

        public async Task RemoveFromQueueAsync(string queueId)
        {
            if (string.IsNullOrEmpty(queueId))
                return;

            try
            {
                var queue = await GetQueueAsync();
                queue.RemoveAll(item => item.QueueId == queueId);

                await WriteQueueAsync(queue);

                System.Diagnostics.Debug.WriteLine(
                    $"[OfflineStorageService] Removed save from queue: {queueId}"
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[OfflineStorageService] Failed to remove save from queue: {ex.Message}"
                );
            }
        }

        public async Task ClearQueueAsync()
        {
            try
            {
                if (File.Exists(_queueFilePath))
                {
                    File.Delete(_queueFilePath);
                }

                System.Diagnostics.Debug.WriteLine("[OfflineStorageService] Offline queue cleared.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[OfflineStorageService] Failed to clear queue: {ex.Message}"
                );
            }
        }

        private async Task WriteQueueAsync(List<QueuedSave> queue)
        {
            try
            {
                var json = JsonSerializer.Serialize(queue, _jsonOptions);
                await File.WriteAllTextAsync(_queueFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[OfflineStorageService] Failed to write queue: {ex.Message}"
                );
                throw;
            }
        }
    }
}
