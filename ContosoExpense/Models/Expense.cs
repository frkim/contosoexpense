namespace ContosoExpense.Models;

/// <summary>
/// Represents an expense in the system.
/// </summary>
public class Expense
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string CategoryId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Draft;
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? ApprovedById { get; set; }
    public string? RejectedById { get; set; }
    public string? PaidById { get; set; }
    public string? ApprovalNotes { get; set; }
    public string? RejectionReason { get; set; }
    public List<string> AttachmentIds { get; set; } = new();
}

/// <summary>
/// Defines the expense lifecycle statuses.
/// </summary>
public enum ExpenseStatus
{
    Draft,
    Submitted,
    Approved,
    Rejected,
    Paid
}
