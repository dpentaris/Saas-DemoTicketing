namespace CareLog.Application.Abstractions;

public interface ITenantContext
{
    Guid TenantId { get; }
    string UserId { get; }
}
