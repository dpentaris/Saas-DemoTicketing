using CareLog.Mobile.Data;

namespace CareLog.Mobile.Services;

public sealed class SyncService(LocalStore store, ApiClient api)
{
    public async Task SyncPendingAsync(CancellationToken ct)
    {
        var pending = await store.GetPendingAsync();
        foreach (var item in pending)
        {
            try
            {
                var result = await api.SyncAsync(new { outboxItems = new[] { new { localId = item.Id, entity = item.Entity, payloadJson = item.PayloadJson, enqueuedAt = DateTimeOffset.UtcNow } } });
                if (result.IsSuccessStatusCode)
                {
                    await store.MarkSentAsync(item.Id);
                }
            }
            catch
            {
                item.RetryCount++;
                var delay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, item.RetryCount), 60));
                await Task.Delay(delay, ct);
            }
        }
    }
}
