using MazeEscape.Models;
using System.Text.Json;

namespace MazeEscape.Services
{
    /// <summary>
    /// Manages offline save queue persistence (stores saves when API is unreachable).
    /// Uses JSON file in AppDataDirectory for simplicity.
    /// </summary>
    public interface IOfflineStorageService
    {
        /// <summary>
        /// Add a save to the offline queue
        /// </summary>
        Task EnqueueSaveAsync(SaveGamePayload payload, string? concurrencyToken, string checkpointName);

        /// <summary>
        /// Retrieve all queued saves
        /// </summary>
        Task<List<QueuedSave>> GetQueueAsync();

        /// <summary>
        /// Remove a save from the queue after successful sync
        /// </summary>
        Task RemoveFromQueueAsync(string queueId);

        /// <summary>
        /// Clear all queued saves (dangerous - use with caution)
        /// </summary>
        Task ClearQueueAsync();
    }

    /// <summary>
    /// Represents a save queued for offline retry
    /// </summary>
    public class QueuedSave
    {
        public required string QueueId { get; set; }
        public required string PlayerId { get; set; }
        public required SaveGamePayload Payload { get; set; }
        public string? ConcurrencyToken { get; set; }
        public required DateTimeOffset SaveTimeUtc { get; set; }
        public required string CheckpointName { get; set; }
        public int RetryCount { get; set; }
    }
}
