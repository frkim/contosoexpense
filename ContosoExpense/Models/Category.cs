namespace ContosoExpense.Models;

/// <summary>
/// Represents an expense category.
/// </summary>
public class Category
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-tag"; // Bootstrap icon class
    public decimal MaxAmountPerExpense { get; set; } = 10000m;
    public decimal MonthlyLimit { get; set; } = 50000m;
    public bool IsActive { get; set; } = true;
    public bool RequiresReceipt { get; set; } = true;
    public decimal ReceiptRequiredThreshold { get; set; } = 50m;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
