using MazeEscape.Models;

namespace MazeEscape.Services
{
    /// <summary>
    /// Type-safe wrapper around HttpClient for all MazeEscape API interactions.
    /// Handles JWT authentication, token refresh on 401, and graceful error handling.
    /// </summary>
    public interface IApiClient
    {
        /// <summary>
        /// Retrieves the player's current save document from the server.
        /// </summary>
        /// <param name="playerId">The player's unique identifier</param>
        /// <returns>SaveDocument if exists; null if 404 (no save found)</returns>
        /// <exception cref="HttpRequestException">On network error or API error (non-404)</exception>
        Task<SaveDocument?> GetSaveAsync(string playerId);

        /// <summary>
        /// Saves or updates the player's game state on the server.
        /// Handles optimistic concurrency with ConcurrencyToken.
        /// </summary>
        /// <param name="playerId">The player's unique identifier</param>
        /// <param name="saveDocument">The save document to persist (includes ConcurrencyToken for conflict detection)</param>
        /// <returns>Updated SaveDocument with new ConcurrencyToken from server</returns>
        /// <exception cref="ConflictException">On HTTP 409 (concurrent modification); contains server's current SaveDocument</exception>
        /// <exception cref="HttpRequestException">On network error or other API error</exception>
        Task<SaveDocument> PutSaveAsync(string playerId, SaveDocument saveDocument);

        /// <summary>
        /// Health check endpoint for monitoring API availability.
        /// </summary>
        /// <returns>Health status object</returns>
        Task<HealthCheckResponse?> GetHealthAsync();
    }

    /// <summary>
    /// Thrown when API returns 409 Conflict during a save attempt.
    /// Contains the server's current SaveDocument for conflict resolution.
    /// </summary>
    public class ConflictException : Exception
    {
        public SaveDocument ServerSaveDocument { get; }

        public ConflictException(string message, SaveDocument serverSaveDocument)
            : base(message)
        {
            ServerSaveDocument = serverSaveDocument;
        }
    }

    /// <summary>
    /// API Health check response
    /// </summary>
    public class HealthCheckResponse
    {
        public string? Status { get; set; }
        public string? Service { get; set; }
        public string? Version { get; set; }
        public string? Environment { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
