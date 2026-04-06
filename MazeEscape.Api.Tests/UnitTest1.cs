using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MazeEscape.Core.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MazeEscape.Api.Tests;

public class SaveApiIntegrationTests
{
    private const string PlayerOneId = "player-001";
    private const string PlayerTwoId = "player-002";

    [Fact]
    public async Task PutWithoutAuthToken_ReturnsUnauthorized()
    {
        using var fixture = new ApiFixture();
        using var client = fixture.CreateClient();

        var document = NewDocument(PlayerOneId, "payload-v1", null);
        var response = await client.PutAsJsonAsync($"/api/saves/{PlayerOneId}", document);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PutWithMismatchedPlayer_ReturnsForbidden()
    {
        using var fixture = new ApiFixture();
        using var client = fixture.CreateClient();
        await fixture.AuthorizeAsync(client, PlayerOneId);

        var document = NewDocument(PlayerTwoId, "payload-v1", null);
        var response = await client.PutAsJsonAsync($"/api/saves/{PlayerTwoId}", document);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetWithMismatchedPlayer_ReturnsForbidden()
    {
        using var fixture = new ApiFixture();
        using var playerOneClient = fixture.CreateClient();
        using var playerTwoClient = fixture.CreateClient();
        await fixture.AuthorizeAsync(playerOneClient, PlayerOneId);
        await fixture.AuthorizeAsync(playerTwoClient, PlayerTwoId);

        var seed = await playerTwoClient.PutAsJsonAsync(
            $"/api/saves/{PlayerTwoId}",
            NewDocument(PlayerTwoId, "payload-player-two", null));
        seed.EnsureSuccessStatusCode();

        var response = await playerOneClient.GetAsync($"/api/saves/{PlayerTwoId}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PutWithStaleConcurrencyToken_ReturnsConflictWithLatestDocument()
    {
        using var fixture = new ApiFixture();
        using var client = fixture.CreateClient();
        await fixture.AuthorizeAsync(client, PlayerOneId);

        var firstPut = await client.PutAsJsonAsync($"/api/saves/{PlayerOneId}", NewDocument(PlayerOneId, "payload-v1", null));
        firstPut.EnsureSuccessStatusCode();
        var firstSaved = await firstPut.Content.ReadFromJsonAsync<SaveDocument>();
        Assert.NotNull(firstSaved);
        Assert.False(string.IsNullOrWhiteSpace(firstSaved!.ConcurrencyToken));

        var stalePut = await client.PutAsJsonAsync(
            $"/api/saves/{PlayerOneId}",
            NewDocument(PlayerOneId, "payload-v2", "stale-token"));

        Assert.Equal(HttpStatusCode.Conflict, stalePut.StatusCode);

        var conflictDoc = await stalePut.Content.ReadFromJsonAsync<SaveDocument>();
        Assert.NotNull(conflictDoc);
        Assert.Equal("payload-v1", conflictDoc!.PayloadJson);
    }

    [Fact]
    public async Task PutWithCurrentConcurrencyToken_OverwritesSuccessfully()
    {
        using var fixture = new ApiFixture();
        using var client = fixture.CreateClient();
        await fixture.AuthorizeAsync(client, PlayerOneId);

        var firstPut = await client.PutAsJsonAsync($"/api/saves/{PlayerOneId}", NewDocument(PlayerOneId, "payload-v1", null));
        firstPut.EnsureSuccessStatusCode();
        var firstSaved = await firstPut.Content.ReadFromJsonAsync<SaveDocument>();
        Assert.NotNull(firstSaved);

        var secondPut = await client.PutAsJsonAsync(
            $"/api/saves/{PlayerOneId}",
            NewDocument(PlayerOneId, "payload-v2", firstSaved!.ConcurrencyToken));

        secondPut.EnsureSuccessStatusCode();
        var secondSaved = await secondPut.Content.ReadFromJsonAsync<SaveDocument>();
        Assert.NotNull(secondSaved);
        Assert.Equal("payload-v2", secondSaved!.PayloadJson);
        Assert.NotEqual(firstSaved.ConcurrencyToken, secondSaved.ConcurrencyToken);
    }

    [Fact]
    public async Task InvalidClientSecret_ReturnsUnauthorized()
    {
        using var fixture = new ApiFixture();
        using var client = fixture.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/token",
            new { playerId = PlayerOneId, clientSecret = "wrong-secret" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static SaveDocument NewDocument(string playerId, string payloadJson, string? concurrencyToken)
    {
        return new SaveDocument
        {
            PlayerId = playerId,
            Version = "1.0",
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            PayloadJson = payloadJson,
            ConcurrencyToken = concurrencyToken
        };
    }

    private sealed class ApiFixture : WebApplicationFactory<Program>
    {
        private readonly string _contentRoot = Path.Combine(Path.GetTempPath(), "mazeescape-api-tests", Guid.NewGuid().ToString("N"));

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(_contentRoot);
            Directory.CreateDirectory(_contentRoot);
            File.WriteAllText(
                Path.Combine(_contentRoot, "appsettings.json"),
                """
                {
                                    "Jwt": {
                                        "Issuer": "MazeEscape.Api.Tests",
                                        "Audience": "MazeEscape.Client.Tests",
                                        "SigningKey": "mazeescape-tests-signing-key-0123456789",
                                        "AccessTokenLifetimeMinutes": 20,
                                        "ClientSecret": "test-client-secret"
                  },
                  "Logging": {
                    "LogLevel": {
                      "Default": "Warning"
                    }
                  }
                }
                """);
        }

        public async Task AuthorizeAsync(HttpClient client, string playerId)
        {
            var tokenResponse = await client.PostAsJsonAsync(
                "/api/auth/token",
                new { playerId, clientSecret = "test-client-secret" });
            tokenResponse.EnsureSuccessStatusCode();

            var token = await tokenResponse.Content.ReadFromJsonAsync<TokenResponseDto>();
            Assert.NotNull(token);
            Assert.False(string.IsNullOrWhiteSpace(token!.AccessToken));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (Directory.Exists(_contentRoot))
            {
                Directory.Delete(_contentRoot, recursive: true);
            }
        }

        private sealed record TokenResponseDto(string AccessToken, DateTimeOffset ExpiresAtUtc);
    }
}
