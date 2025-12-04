using ContosoExpense.Models;

namespace ContosoExpense.Services;

/// <summary>
/// Interface for attachment repository operations.
/// </summary>
public interface IAttachmentRepository
{
    Task<IEnumerable<Attachment>> GetByExpenseIdAsync(string expenseId);
    Task<Attachment?> GetByIdAsync(string id);
    Task<Attachment> CreateAsync(Attachment attachment);
    Task<bool> DeleteAsync(string id);
    void Reset();
}

/// <summary>
/// In-memory implementation of attachment repository.
/// </summary>
public class InMemoryAttachmentRepository : IAttachmentRepository
{
    private readonly List<Attachment> _attachments = new();
    private readonly object _lock = new();

    public void Reset()
    {
        lock (_lock)
        {
            _attachments.Clear();
        }
    }

    public Task<IEnumerable<Attachment>> GetByExpenseIdAsync(string expenseId)
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<Attachment>>(
                _attachments.Where(a => a.ExpenseId == expenseId).ToList());
        }
    }

    public Task<Attachment?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            return Task.FromResult(_attachments.FirstOrDefault(a => a.Id == id));
        }
    }

    public Task<Attachment> CreateAsync(Attachment attachment)
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(attachment.Id))
                attachment.Id = Guid.NewGuid().ToString();
            _attachments.Add(attachment);
            return Task.FromResult(attachment);
        }
    }

    public Task<bool> DeleteAsync(string id)
    {
        lock (_lock)
        {
            var attachment = _attachments.FirstOrDefault(a => a.Id == id);
            if (attachment != null)
            {
                _attachments.Remove(attachment);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
