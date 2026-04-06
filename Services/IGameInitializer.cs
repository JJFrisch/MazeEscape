using MazeEscape.Models;

namespace MazeEscape.Services
{
    /// <summary>
    /// Initializes or loads game state from API. Detects first-time players by checking for existing save.
    /// Stores current save document for later use by SaveSynchronizer (concurrency token).
    /// </summary>
    public interface IGameInitializer
    {
        /// <summary>
        /// Checks API for existing save. If found, loads it into playerData.
        /// If not found (404), initializes playerData with defaults.
        /// </summary>
        /// <returns>Result with initialization status and current SaveDocument</returns>
        Task<GameInitResult> InitializeOrLoadAsync(PlayerDataModel playerData);

        /// <summary>
        /// Get the last-loaded or initialized SaveDocument (includes concurrency token for next save)
        /// </summary>
        SaveDocument? GetCurrentSaveDocument();
    }

    public class GameInitResult
    {
        public bool IsFirstTime { get; set; }
        public SaveDocument? CurrentSave { get; set; }
        public string? ErrorMessage { get; set; }
        public Exception? Exception { get; set; }
    }
}
