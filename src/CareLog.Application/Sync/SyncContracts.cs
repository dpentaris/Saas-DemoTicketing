namespace CareLog.Application.Sync;

public sealed record OutboxItemDto(Guid LocalId, string Entity, string PayloadJson, DateTimeOffset EnqueuedAt);
public sealed record SyncRequest(IReadOnlyCollection<OutboxItemDto> OutboxItems);
public sealed record SyncResult(Guid LocalId, bool Success, string? Warning);
