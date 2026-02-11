namespace CareLog.Mobile.Models;

public sealed class VisitItem
{
    public Guid Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public DateTimeOffset ScheduledStart { get; set; }
    public string SyncStatus { get; set; } = "Synced";
}
