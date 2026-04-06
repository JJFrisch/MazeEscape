using System.Text.Json;
using System.Text.Json.Serialization;

namespace MazeEscape.Services
{
    /// <summary>
    /// Implements JWT token lifecycle management. Obtains JWT from API, refreshes before expiration,
    /// and securely stores tokens using device SecureStorage.
    /// </summary>
    public class AuthTokenProvider : IAuthTokenProvider
    {
        private const string STORAGE_KEY_TOKEN = "jwt_access_token";
        private const string STORAGE_KEY_EXPIRATION = "jwt_expiration_utc";
        private const int TOKEN_REFRESH_BUFFER_MINUTES = 5; // Refresh if < 5 min remaining

        private readonly HttpClient _httpClient;
        private readonly string _playerId;
        private readonly string _clientSecret;

        public AuthTokenProvider(HttpClient httpClient, string playerId, string clientSecret)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _playerId = playerId ?? throw new ArgumentNullException(nameof(playerId));
            _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
        }

        public async Task<string> GetValidTokenAsync()
        {
            // Try to get existing token from secure storage
            var existingToken = await SecureStorage.GetAsync(STORAGE_KEY_TOKEN);
            var expirationStr = await SecureStorage.GetAsync(STORAGE_KEY_EXPIRATION);

            // Check if existing token is still valid (not expired, at least 5 min remaining)
            if (!string.IsNullOrEmpty(existingToken) && !string.IsNullOrEmpty(expirationStr))
            {
                if (DateTimeOffset.TryParse(expirationStr, out var expiration))
                {
                    var timeUntilExpiration = expiration - DateTimeOffset.UtcNow;
                    if (timeUntilExpiration.TotalMinutes > TOKEN_REFRESH_BUFFER_MINUTES)
                    {
                        // Token is still valid
                        return existingToken;
                    }
                }
            }

            // Token is missing, expired, or will expire soon → refresh it
            return await RefreshTokenAsync();
        }

        public async Task ClearTokenAsync()
        {
            try
            {
                await SecureStorage.Remove(STORAGE_KEY_TOKEN);
                await SecureStorage.Remove(STORAGE_KEY_EXPIRATION);
            }
            catch
            {
                // Ignore errors on token cleanup
            }
        }

        private async Task<string> RefreshTokenAsync()
        {
            try
            {
                var request = new TokenRequest(_playerId, _clientSecret);
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("/api/auth/token", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException(
                        $"Failed to obtain JWT token. Status: {response.StatusCode}, Content: {errorContent}",
                        null
                    );
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    throw new InvalidOperationException("API returned invalid token response (missing AccessToken)");
                }

                // Store token and expiration securely
                await SecureStorage.SetAsync(STORAGE_KEY_TOKEN, tokenResponse.AccessToken);
                await SecureStorage.SetAsync(STORAGE_KEY_EXPIRATION, tokenResponse.ExpiresAtUtc.ToString("O"));

                return tokenResponse.AccessToken;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthTokenProvider] Token refresh failed: {ex.Message}");
                throw;
            }
        }

        // API Contract DTOs
        #region API Contracts

        [Serializable]
        private sealed record TokenRequest(
            [property: JsonPropertyName("playerId")] string PlayerId,
            [property: JsonPropertyName("clientSecret")] string ClientSecret
        );

        [Serializable]
        private sealed record TokenResponse(
            [property: JsonPropertyName("accessToken")] string AccessToken,
            [property: JsonPropertyName("expiresAtUtc")] DateTimeOffset ExpiresAtUtc
        );

        #endregion
    }
}
