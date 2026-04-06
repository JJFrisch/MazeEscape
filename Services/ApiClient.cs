using System.Text.Json;
using MazeEscape.Models;

namespace MazeEscape.Services
{
    /// <summary>
    /// Type-safe HTTP client wrapper for MazeEscape.Api.
    /// Handles JWT authentication, token refresh, and conflict resolution.
    /// </summary>
    public class ApiClient : IApiClient
    {
        private const int MAX_RETRY_ATTEMPTS = 3;
        private readonly int[] RETRY_DELAYS_MS = { 500, 1000, 2000 }; // Exponential backoff: 500ms, 1s, 2s

        private readonly HttpClient _httpClient;
        private readonly IAuthTokenProvider _tokenProvider;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiClient(HttpClient httpClient, IAuthTokenProvider tokenProvider)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<SaveDocument?> GetSaveAsync(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
                throw new ArgumentException("PlayerId cannot be null or empty", nameof(playerId));

            try
            {
                var token = await _tokenProvider.GetValidTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/saves/{playerId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // 404 = no save found (first-time player)
                    return null;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // 401 = token expired, clear and retry once
                    await _tokenProvider.ClearTokenAsync();
                    token = await _tokenProvider.GetValidTokenAsync();
                    request = new HttpRequestMessage(HttpMethod.Get, $"/api/saves/{playerId}");
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    response = await _httpClient.SendAsync(request);
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SaveDocument>(content, _jsonOptions);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new InvalidOperationException("Authentication failed. Token refresh unsuccessful.", ex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiClient] GetSaveAsync failed: {ex.Message}");
                throw;
            }
        }

        public async Task<SaveDocument> PutSaveAsync(string playerId, SaveDocument saveDocument)
        {
            if (string.IsNullOrEmpty(playerId))
                throw new ArgumentException("PlayerId cannot be null or empty", nameof(playerId));

            ArgumentNullException.ThrowIfNull(saveDocument);

            var json = JsonSerializer.Serialize(saveDocument, _jsonOptions);
            int attemptCount = 0;

            while (attemptCount < MAX_RETRY_ATTEMPTS)
            {
                try
                {
                    var token = await _tokenProvider.GetValidTokenAsync();
                    var request = new HttpRequestMessage(HttpMethod.Put, $"/api/saves/{playerId}");
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var response = await _httpClient.SendAsync(request);

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        // 401 = token expired, refresh and retry
                        await _tokenProvider.ClearTokenAsync();
                        attemptCount++;
                        
                        if (attemptCount < MAX_RETRY_ATTEMPTS)
                        {
                            await Task.Delay(RETRY_DELAYS_MS[attemptCount - 1]);
                            continue;
                        }
                        else
                        {
                            throw new InvalidOperationException("Authentication failed after retry. Token refresh unsuccessful.");
                        }
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        // 409 = ConcurrencyToken mismatch
                        var conflictContent = await response.Content.ReadAsStringAsync();
                        var serverDoc = JsonSerializer.Deserialize<SaveDocument>(conflictContent, _jsonOptions);

                        throw new ConflictException(
                            $"Save conflict detected. Server has newer version (token: {serverDoc?.ConcurrencyToken})",
                            serverDoc!
                        );
                    }

                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<SaveDocument>(responseContent, _jsonOptions);

                    if (result == null)
                        throw new InvalidOperationException("API returned null SaveDocument");

                    return result;
                }
                catch (ConflictException)
                {
                    // Re-throw conflict exceptions immediately (don't retry)
                    throw;
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.RequestTimeout || 
                                                      (int?)ex.StatusCode >= 500)
                {
                    // Retry on network timeout or 5xx errors
                    attemptCount++;

                    if (attemptCount < MAX_RETRY_ATTEMPTS)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[ApiClient] PutSaveAsync attempt {attemptCount} failed. Retrying in {RETRY_DELAYS_MS[attemptCount - 1]}ms..."
                        );
                        await Task.Delay(RETRY_DELAYS_MS[attemptCount - 1]);
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ApiClient] PutSaveAsync failed: {ex.Message}");
                    throw;
                }
            }

            throw new InvalidOperationException("PutSaveAsync exhausted all retry attempts");
        }

        public async Task<HealthCheckResponse?> GetHealthAsync()
        {
            try
            {
                // Health check doesn't require auth
                var response = await _httpClient.GetAsync("/api/health");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<HealthCheckResponse>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiClient] Health check failed: {ex.Message}");
                return null;
            }
        }
    }
}
