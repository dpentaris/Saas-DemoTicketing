using SQLite;

namespace CareLog.Mobile.Data;

public sealed class LocalStore
{
    private SQLiteAsyncConnection? _db;

    public async Task InitAsync(string path)
    {
        _db = new SQLiteAsyncConnection(path);
        await _db.CreateTableAsync<OutboxItem>();
    }

    public Task<int> EnqueueAsync(OutboxItem item) => _db!.InsertAsync(item);
    public Task<List<OutboxItem>> GetPendingAsync() => _db!.Table<OutboxItem>().Where(x => !x.Sent).ToListAsync();
    public Task<int> MarkSentAsync(Guid id) => _db!.ExecuteAsync("UPDATE OutboxItem SET Sent = 1 WHERE Id = ?", id);
}

public sealed class OutboxItem
{
    [PrimaryKey] public Guid Id { get; set; }
    public string Entity { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public bool Sent { get; set; }
}
