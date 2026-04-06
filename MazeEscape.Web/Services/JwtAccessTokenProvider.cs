using System.Net.Http.Json;

namespace MazeEscape.Web.Services;

public sealed class JwtAccessTokenProvider : IAccessTokenProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    private string? _cachedToken;
    private DateTimeOffset _expiresAtUtc;

    public JwtAccessTokenProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(_cachedToken) && DateTimeOffset.UtcNow < _expiresAtUtc.AddSeconds(-30))
        {
            return _cachedToken;
        }

        var playerId = _configuration["SyncApi:PlayerId"] ?? "player-001";
        var clientSecret = _configuration["SyncApi:ClientSecret"] ?? string.Empty;

        var response = await _httpClient.PostAsJsonAsync(
            "api/auth/token",
            new TokenRequest(playerId, clientSecret),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Token response was empty.");

        _cachedToken = payload.AccessToken;
        _expiresAtUtc = payload.ExpiresAtUtc;
        return _cachedToken;
    }

    private sealed record TokenRequest(string PlayerId, string ClientSecret);
    private sealed record TokenResponse(string AccessToken, DateTimeOffset ExpiresAtUtc);
}
