using System.Net;
using System.Net.Http.Json;
using MazeEscape.Core.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MazeEscape.Api.Tests;

public class SaveApiIntegrationTests
{
    private const string ValidToken = "dev-player-001-token";
    private const string InvalidToken = "invalid-token";
    private const string PlayerId = "player-001";

    [Fact]
    public async Task PutWithoutAuthToken_ReturnsUnauthorized()
    {
        using var fixture = new ApiFixture();
        using var client = fixture.CreateClient();

        var document = NewDocument(PlayerId, "payload-v1", null);
        var response = await client.PutAsJsonAsync($"/api/saves/{PlayerId}", document);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PutWithMismatchedPlayer_ReturnsForbidden()
    {
        using var fixture = new ApiFixture();
        using var client = fixture.CreateAuthorizedClient(ValidToken);

        var document = NewDocument("player-002", "payload-v1", null);
        var response = await client.PutAsJsonAsync("/api/saves/player-002", document);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PutWithStaleConcurrencyToken_ReturnsConflictWithLatestDocument()
    {
        using var fixture = new ApiFixture();
        using var client = fixture.CreateAuthorizedClient(ValidToken);

        var firstPut = await client.PutAsJsonAsync($"/api/saves/{PlayerId}", NewDocument(PlayerId, "payload-v1", null));
        firstPut.EnsureSuccessStatusCode();
        var firstSaved = await firstPut.Content.ReadFromJsonAsync<SaveDocument>();
        Assert.NotNull(firstSaved);
        Assert.False(string.IsNullOrWhiteSpace(firstSaved!.ConcurrencyToken));

        var stalePut = await client.PutAsJsonAsync(
            $"/api/saves/{PlayerId}",
            NewDocument(PlayerId, "payload-v2", "stale-token"));

        Assert.Equal(HttpStatusCode.Conflict, stalePut.StatusCode);

        var conflictDoc = await stalePut.Content.ReadFromJsonAsync<SaveDocument>();
        Assert.NotNull(conflictDoc);
        Assert.Equal("payload-v1", conflictDoc!.PayloadJson);
    }

    [Fact]
    public async Task PutWithCurrentConcurrencyToken_OverwritesSuccessfully()
    {
        using var fixture = new ApiFixture();
        using var client = fixture.CreateAuthorizedClient(ValidToken);

        var firstPut = await client.PutAsJsonAsync($"/api/saves/{PlayerId}", NewDocument(PlayerId, "payload-v1", null));
        firstPut.EnsureSuccessStatusCode();
        var firstSaved = await firstPut.Content.ReadFromJsonAsync<SaveDocument>();
        Assert.NotNull(firstSaved);

        var secondPut = await client.PutAsJsonAsync(
            $"/api/saves/{PlayerId}",
            NewDocument(PlayerId, "payload-v2", firstSaved!.ConcurrencyToken));

        secondPut.EnsureSuccessStatusCode();
        var secondSaved = await secondPut.Content.ReadFromJsonAsync<SaveDocument>();
        Assert.NotNull(secondSaved);
        Assert.Equal("payload-v2", secondSaved!.PayloadJson);
        Assert.NotEqual(firstSaved.ConcurrencyToken, secondSaved.ConcurrencyToken);
    }

    [Fact]
    public async Task InvalidToken_ReturnsUnauthorized()
    {
        using var fixture = new ApiFixture();
        using var client = fixture.CreateAuthorizedClient(InvalidToken);

        var response = await client.GetAsync($"/api/saves/{PlayerId}");
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
                  "SyncAuth": {
                    "Tokens": {
                      "dev-player-001-token": "player-001",
                      "dev-player-002-token": "player-002"
                    }
                  },
                  "Logging": {
                    "LogLevel": {
                      "Default": "Warning"
                    }
                  }
                }
                """);
        }

        public HttpClient CreateAuthorizedClient(string token)
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Add("X-Player-Token", token);
            return client;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (Directory.Exists(_contentRoot))
            {
                Directory.Delete(_contentRoot, recursive: true);
            }
        }
    }
}
