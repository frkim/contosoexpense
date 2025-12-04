namespace ContosoExpense.Models;

/// <summary>
/// Represents a comment on an expense.
/// </summary>
public class ExpenseComment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ExpenseId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
