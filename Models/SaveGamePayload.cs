namespace MazeEscape.Models
{
    /// <summary>
    /// Game state DTO that gets serialized into SaveDocument.PayloadJson.
    /// Contains all game data needed to restore player progress.
    /// </summary>
    public class SaveGamePayload
    {
        // Player identity & metadata
        public required string PlayerId { get; set; }
        public required string PlayerName { get; set; }

        // Currency & inventory
        public int CoinCount { get; set; }
        public int HintsOwned { get; set; }
        public int ExtraTimesOwned { get; set; }
        public int ExtraMovesOwned { get; set; }

        // World & level progression
        public required List<WorldProgressPayload> Worlds { get; set; }

        // Cosmetics
        public required List<string> UnlockedSkinIds { get; set; }
        public required string CurrentSkinId { get; set; }

        // Achievements
        public bool MonthPrize1_achieved { get; set; }
        public bool MonthPrize2_achieved { get; set; }
        public string? MostRecentMonth { get; set; }

        // Display settings
        public string? WallColorHex { get; set; }

        // Schema versioning (for future migrations)
        public required string SchemaVersion { get; set; }

        // Client-side save timestamp (for debugging)
        public required DateTimeOffset LocalSaveTimeUtc { get; set; }

        // Daily maze database reference (if needed)
        public string? DailyMazeDatabaseJson { get; set; }
    }

    /// <summary>
    /// World progression data (one per world)
    /// </summary>
    public class WorldProgressPayload
    {
        public required string WorldId { get; set; }
        public int StarsEarned { get; set; }
        public required List<LevelProgressPayload> Levels { get; set; }
    }

    /// <summary>
    /// Level progression data (one per level)
    /// </summary>
    public class LevelProgressPayload
    {
        public required int LevelId { get; set; }
        public bool IsCompleted { get; set; }
        public int BestMovesUsed { get; set; }
        public int BestTimeSeconds { get; set; }
        public int StarsEarned { get; set; }
    }
}
