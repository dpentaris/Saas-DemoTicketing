using CareLog.Application.Abstractions;

namespace CareLog.Infrastructure.Notifications;

public sealed class NoOpNotificationService : INotificationService
{
    public Task NotifyVisitChangedAsync(Guid visitId, string message, CancellationToken ct) => Task.CompletedTask;
}
