namespace MazeEscape.Web.Services;

public sealed class SyncStatusService
{
    public event Action? Changed;

    public bool IsSyncing { get; private set; }
    public string StatusText { get; private set; } = "Idle";
    public DateTimeOffset? LastUpdatedUtc { get; private set; }

    public void SetSyncing(string message)
    {
        IsSyncing = true;
        StatusText = message;
        LastUpdatedUtc = DateTimeOffset.UtcNow;
        Changed?.Invoke();
    }

    public void SetIdle(string message)
    {
        IsSyncing = false;
        StatusText = message;
        LastUpdatedUtc = DateTimeOffset.UtcNow;
        Changed?.Invoke();
    }
}
