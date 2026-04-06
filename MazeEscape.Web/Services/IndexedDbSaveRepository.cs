using System.Text.Json;
using MazeEscape.Core.Persistence;
using Microsoft.JSInterop;

namespace MazeEscape.Web.Services;

public sealed class IndexedDbSaveRepository : ISaveRepository
{
    private readonly IJSRuntime _js;

    public IndexedDbSaveRepository(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<SaveDocument?> LoadAsync(string playerId, CancellationToken cancellationToken = default)
    {
        var payloadJson = await _js.InvokeAsync<string?>("mazeEscapeDb.loadDocument", cancellationToken, playerId);
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return null;
        }

        return JsonSerializer.Deserialize<SaveDocument>(payloadJson);
    }

    public async Task SaveAsync(SaveDocument document, CancellationToken cancellationToken = default)
    {
        var payloadJson = JsonSerializer.Serialize(document);
        await _js.InvokeVoidAsync("mazeEscapeDb.saveDocument", cancellationToken, document.PlayerId, payloadJson);
    }
}
