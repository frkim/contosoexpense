using ContosoExpense.Models;

namespace ContosoExpense.Services;

/// <summary>
/// Interface for expense comment operations.
/// </summary>
public interface IExpenseCommentRepository
{
    Task<IEnumerable<ExpenseComment>> GetByExpenseIdAsync(string expenseId);
    Task<ExpenseComment> CreateAsync(ExpenseComment comment);
    Task<bool> DeleteAsync(string id);
    void Reset();
}

/// <summary>
/// In-memory implementation of expense comment repository.
/// </summary>
public class InMemoryExpenseCommentRepository : IExpenseCommentRepository
{
    private readonly List<ExpenseComment> _comments = new();
    private readonly object _lock = new();

    public void Reset()
    {
        lock (_lock)
        {
            _comments.Clear();
        }
    }

    public Task<IEnumerable<ExpenseComment>> GetByExpenseIdAsync(string expenseId)
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<ExpenseComment>>(
                _comments.Where(c => c.ExpenseId == expenseId)
                         .OrderBy(c => c.CreatedAt)
                         .ToList());
        }
    }

    public Task<ExpenseComment> CreateAsync(ExpenseComment comment)
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(comment.Id))
                comment.Id = Guid.NewGuid().ToString();
            _comments.Add(comment);
            return Task.FromResult(comment);
        }
    }

    public Task<bool> DeleteAsync(string id)
    {
        lock (_lock)
        {
            var comment = _comments.FirstOrDefault(c => c.Id == id);
            if (comment != null)
            {
                _comments.Remove(comment);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
