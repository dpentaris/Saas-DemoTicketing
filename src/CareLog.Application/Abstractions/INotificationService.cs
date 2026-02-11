namespace CareLog.Application.Abstractions;

public interface INotificationService
{
    Task NotifyVisitChangedAsync(Guid visitId, string message, CancellationToken ct);
}
