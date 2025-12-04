using ContosoExpense.Models;

namespace ContosoExpense.Services;

/// <summary>
/// Interface for audit log operations.
/// </summary>
public interface IAuditLogRepository
{
    Task<IEnumerable<AuditLog>> GetAllAsync();
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, string entityId);
    Task<AuditLog> CreateAsync(AuditLog log);
    void Reset();
}

/// <summary>
/// In-memory implementation of audit log repository.
/// </summary>
public class InMemoryAuditLogRepository : IAuditLogRepository
{
    private readonly List<AuditLog> _logs = new();
    private readonly object _lock = new();

    public void Reset()
    {
        lock (_lock)
        {
            _logs.Clear();
        }
    }

    public Task<IEnumerable<AuditLog>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<AuditLog>>(_logs.OrderByDescending(l => l.Timestamp).ToList());
        }
    }

    public Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, string entityId)
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<AuditLog>>(
                _logs.Where(l => l.EntityType == entityType && l.EntityId == entityId)
                     .OrderByDescending(l => l.Timestamp)
                     .ToList());
        }
    }

    public Task<AuditLog> CreateAsync(AuditLog log)
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(log.Id))
                log.Id = Guid.NewGuid().ToString();
            _logs.Add(log);
            return Task.FromResult(log);
        }
    }
}
