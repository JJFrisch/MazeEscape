using System.Text.Json.Serialization;

namespace MazeEscape.Models
{
    /// <summary>
    /// API contract for saving and loading game state.
    /// Mirrors the backend SaveDocument structure from MazeEscape.Api.
    /// PayloadJson field contains serialized SaveGamePayload.
    /// </summary>
    public class SaveDocument
    {
        [JsonPropertyName("playerId")]
        public required string PlayerId { get; set; }

        [JsonPropertyName("version")]
        public required string Version { get; set; }

        [JsonPropertyName("updatedAtUtc")]
        public required DateTimeOffset UpdatedAtUtc { get; set; }

        [JsonPropertyName("payloadJson")]
        public required string PayloadJson { get; set; }

        [JsonPropertyName("concurrencyToken")]
        public string? ConcurrencyToken { get; set; }
    }
}
