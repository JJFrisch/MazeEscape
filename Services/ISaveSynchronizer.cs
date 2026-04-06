using MazeEscape.Models;

namespace MazeEscape.Services
{
    /// <summary>
    /// Manages save-to-server operations with conflict resolution and offline queue fallback.
    /// Handles checkpoint saves triggered by game events (level complete, item purchase, etc.).
    /// On conflict or network error, queues save for later sync.
    /// </summary>
    public interface ISaveSynchronizer
    {
        /// <summary>
        /// Saves current game state to API and updates concurrency token.
        /// On conflict, retries with server's new token. On network error, queues for later.
        /// </summary>
        /// <param name="playerData">Current game state to serialize</param>
        /// <param name="checkpointName">Debug label (e.g., "level_completed", "item_purchased")</param>
        Task SaveCheckpointAsync(PlayerDataModel playerData, string checkpointName);

        /// <summary>
        /// Processes offline save queue (called on app resume or connectivity restored).
        /// Retries all queued saves with exponential backoff.
        /// </summary>
        Task ProcessOfflineQueueAsync();

        /// <summary>
        /// Get count of pending offline saves
        /// </summary>
        Task<int> GetOfflineQueueCountAsync();
    }
}
