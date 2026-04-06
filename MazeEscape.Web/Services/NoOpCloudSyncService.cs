using MazeEscape.Core.Persistence;
using MazeEscape.Core.Sync;
using System.Net;
using System.Net.Http.Json;

namespace MazeEscape.Web.Services;

public sealed class NoOpCloudSyncService : ICloudSyncService
{
    private readonly HttpClient _httpClient;

    public NoOpCloudSyncService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<SaveDocument?> PullAsync(string playerId, CancellationToken cancellationToken = default)
    {
        return PullInternalAsync(playerId, cancellationToken);
    }

    public Task<SyncResult> PushAsync(SaveDocument document, CancellationToken cancellationToken = default)
    {
        return PushInternalAsync(document, cancellationToken);
    }

    private async Task<SaveDocument?> PullInternalAsync(string playerId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/saves/{Uri.EscapeDataString(playerId)}", cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SaveDocument>(cancellationToken: cancellationToken);
        }
        catch
        {
            // Keep load behavior resilient. If cloud is unavailable, callers can still continue with local saves.
            return null;
        }
    }

    private async Task<SyncResult> PushInternalAsync(SaveDocument document, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.PutAsJsonAsync(
                $"api/saves/{Uri.EscapeDataString(document.PlayerId)}",
                document,
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                var latest = await response.Content.ReadFromJsonAsync<SaveDocument>(cancellationToken: cancellationToken);
                var serverVersion = latest?.UpdatedAtUtc.ToString("O") ?? "unknown";
                return new SyncResult
                {
                    Success = false,
                    ConflictDetected = true,
                    Message = $"Cloud sync conflict detected. Server has newer data from {serverVersion}."
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                return new SyncResult
                {
                    Success = false,
                    ConflictDetected = false,
                    Message = $"Cloud sync failed: {(int)response.StatusCode} {response.ReasonPhrase}"
                };
            }

            return new SyncResult
            {
                Success = true,
                ConflictDetected = false,
                Message = "Cloud sync completed."
            };
        }
        catch (Exception ex)
        {
            return new SyncResult
            {
                Success = false,
                ConflictDetected = false,
                Message = $"Cloud sync failed: {ex.Message}"
            };
        }
    }
}
