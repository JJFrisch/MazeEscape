using MazeEscape.Models;

namespace MazeEscape.Services
{
    /// <summary>
    /// Implements game state initialization/loading from API.
    /// Handles first-time player detection and default initialization.
    /// </summary>
    public class GameInitializer : IGameInitializer
    {
        private readonly IApiClient _apiClient;
        private SaveDocument? _currentSaveDocument;

        public GameInitializer(IApiClient apiClient)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        public async Task<GameInitResult> InitializeOrLoadAsync(PlayerDataModel playerData)
        {
            ArgumentNullException.ThrowIfNull(playerData);

            try
            {
                System.Diagnostics.Debug.WriteLine($"[GameInitializer] Initializing player {playerData.PlayerId}");

                // Try to load existing save from API
                var saveDocument = await _apiClient.GetSaveAsync(playerData.PlayerId.ToString());

                if (saveDocument != null)
                {
                    // Existing save found → Load it
                    System.Diagnostics.Debug.WriteLine($"[GameInitializer] Existing save found. Loading...");
                    
                    var payload = GameStateMapper.DeserializePayload(saveDocument.PayloadJson);
                    if (payload != null)
                    {
                        GameStateMapper.ApplyPayload(playerData, payload);
                        _currentSaveDocument = saveDocument;

                        return new GameInitResult
                        {
                            IsFirstTime = false,
                            CurrentSave = saveDocument,
                            ErrorMessage = null
                        };
                    }
                    else
                    {
                        // Save exists but payload is corrupted
                        return new GameInitResult
                        {
                            IsFirstTime = true,
                            CurrentSave = null,
                            ErrorMessage = "Existing save found but payload is corrupted. Initializing with defaults."
                        };
                    }
                }
                else
                {
                    // No save found (404) → First-time player
                    System.Diagnostics.Debug.WriteLine($"[GameInitializer] No existing save (404). Initializing as first-time player.");
                    
                    InitializeDefaults(playerData);
                    _currentSaveDocument = null; // Will be created on first save

                    return new GameInitResult
                    {
                        IsFirstTime = true,
                        CurrentSave = null,
                        ErrorMessage = null
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GameInitializer] Initialization failed: {ex.Message}");

                // On error, initialize with defaults (offline first-time experience)
                InitializeDefaults(playerData);

                return new GameInitResult
                {
                    IsFirstTime = true,
                    CurrentSave = null,
                    ErrorMessage = $"Failed to load from API: {ex.Message}",
                    Exception = ex
                };
            }
        }

        public SaveDocument? GetCurrentSaveDocument() => _currentSaveDocument;

        private static void InitializeDefaults(PlayerDataModel playerData)
        {
            // Ensure basic properties are set
            if (string.IsNullOrEmpty(playerData.PlayerName))
            {
                playerData.PlayerName = "Player";
            }

            // CoinCount and inventory already initialized to 0 by PlayerDataModel

            // Note: Worlds should already be initialized by PlayerDataModel.InitializeWorlds()
            // This is called in PlayerDataModel constructor
        }
    }
}
