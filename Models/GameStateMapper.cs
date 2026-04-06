using System.Text.Json;

namespace MazeEscape.Models
{
    /// <summary>
    /// Handles bidirectional mapping between PlayerDataModel domain objects and SaveGamePayload DTOs.
    /// Provides serialization/deserialization for API communication.
    /// </summary>
    public static class GameStateMapper
    {
        private const string SCHEMA_VERSION = "1.0";

        /// <summary>
        /// Converts PlayerDataModel to SaveGamePayload for API serialization.
        /// </summary>
        public static SaveGamePayload ToPayload(PlayerDataModel playerData)
        {
            ArgumentNullException.ThrowIfNull(playerData);

            // Get current skin ID or default
            var currentSkinId = playerData.PlayerCurrentSkin?.Name ?? "default";

            // Extract skin IDs from UnlockedSkins
            var unlockedSkinIds = playerData.UnlockedSkins?
                .Select(s => s.Name ?? "unknown")
                .ToList() ?? new List<string>();

            // Build worlds snapshot
            var worldsPayload = new List<WorldProgressPayload>();
            if (playerData.Worlds != null && playerData.Worlds.Count > 0)
            {
                foreach (var world in playerData.Worlds)
                {
                    worldsPayload.Add(new WorldProgressPayload
                    {
                        WorldId = world.WorldID.ToString(),
                        StarsEarned = world.StarCount,
                        Levels = new List<LevelProgressPayload>() // Note: complex level data preserved in raw JSON
                    });
                }
            }

            // Serialize the full SaveableData to JSON for backup
            var saveableData = new PlayerDataModel.SaveableData
            {
                PlayerName = playerData.PlayerName,
                Worlds = playerData.Worlds,
                CurrentWorldIndex = playerData.CurrentWorldIndex,
                CoinCount = playerData.CoinCount,
                HintsOwned = playerData.HintsOwned,
                ExtraTimesOwned = playerData.ExtraTimesOwned,
                ExtraMovesOwned = playerData.ExtraMovesOwned,
                PlayerCurrentSkin = playerData.PlayerCurrentSkin,
                MonthPrize1_achieved = playerData.MonthPrize1_achieved,
                MonthPrize2_achieved = playerData.MonthPrize2_achieved,
                MostRecentMonth = playerData.MostRecentMonth,
                UnlockedSkins = playerData.UnlockedSkins,
                WallColor = playerData.WallColor
            };

            return new SaveGamePayload
            {
                PlayerId = playerData.PlayerId.ToString(),
                PlayerName = playerData.PlayerName ?? "Player",
                CoinCount = playerData.CoinCount,
                HintsOwned = playerData.HintsOwned,
                ExtraTimesOwned = playerData.ExtraTimesOwned,
                ExtraMovesOwned = playerData.ExtraMovesOwned,
                Worlds = worldsPayload,
                UnlockedSkinIds = unlockedSkinIds,
                CurrentSkinId = currentSkinId,
                MonthPrize1_achieved = playerData.MonthPrize1_achieved,
                MonthPrize2_achieved = playerData.MonthPrize2_achieved,
                MostRecentMonth = playerData.MostRecentMonth,
                WallColorHex = playerData.WallColor?.ToHex() ?? "#000000",
                SchemaVersion = SCHEMA_VERSION,
                LocalSaveTimeUtc = DateTimeOffset.UtcNow,
                // Store the full SaveableData as backup JSON
                SaveableDataBackupJson = JsonSerializer.Serialize(saveableData, new JsonSerializerOptions { WriteIndented = false })
            };
        }

        /// <summary>
        /// Applies SaveGamePayload to PlayerDataModel (deserializes API data into domain model).
        /// </summary>
        public static void ApplyPayload(PlayerDataModel playerData, SaveGamePayload payload)
        {
            ArgumentNullException.ThrowIfNull(playerData);
            ArgumentNullException.ThrowIfNull(payload);

            // Update basic properties
            playerData.PlayerName = payload.PlayerName ?? "Player";
            playerData.CoinCount = payload.CoinCount;
            playerData.HintsOwned = payload.HintsOwned;
            playerData.ExtraTimesOwned = payload.ExtraTimesOwned;
            playerData.ExtraMovesOwned = payload.ExtraMovesOwned;
            playerData.MonthPrize1_achieved = payload.MonthPrize1_achieved;
            playerData.MonthPrize2_achieved = payload.MonthPrize2_achieved;
            playerData.MostRecentMonth = payload.MostRecentMonth;

            // Update color
            if (!string.IsNullOrEmpty(payload.WallColorHex))
            {
                try
                {
                    playerData.WallColor = Color.FromArgb(payload.WallColorHex);
                }
                catch
                {
                    playerData.WallColor = Colors.Black;
                }
            }

            // Try to restore from backup SaveableData if available
            if (!string.IsNullOrEmpty(payload.SaveableDataBackupJson))
            {
                try
                {
                    var saveableData = JsonSerializer.Deserialize<PlayerDataModel.SaveableData>(
                        payload.SaveableDataBackupJson
                    );

                    if (saveableData != null)
                    {
                        if (saveableData.Worlds != null)
                            playerData.Worlds = saveableData.Worlds;
                        if (saveableData.UnlockedSkins != null)
                            playerData.UnlockedSkins = saveableData.UnlockedSkins;
                        playerData.CurrentWorldIndex = saveableData.CurrentWorldIndex;
                    }
                }
                catch
                {
                    // Ignore backup restoration errors
                }
            }
        }

        /// <summary>
        /// Serializes SaveGamePayload to JSON string for storage in SaveDocument.PayloadJson
        /// </summary>
        public static string SerializePayload(SaveGamePayload payload)
        {
            ArgumentNullException.ThrowIfNull(payload);
            return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = false });
        }

        /// <summary>
        /// Deserializes SaveGamePayload from JSON string.
        /// </summary>
        public static SaveGamePayload? DeserializePayload(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<SaveGamePayload>(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GameStateMapper] Failed to deserialize payload: {ex.Message}");
                return null;
            }
        }
    }
}
