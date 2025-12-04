namespace ContosoExpense.Models;

/// <summary>
/// System settings for approval thresholds and currencies.
/// </summary>
public class SystemSettings
{
    public decimal AutoApprovalThreshold { get; set; } = 0m; // 0 means no auto-approval
    public decimal ReceiptRequiredThreshold { get; set; } = 50m;
    public List<string> AllowedCurrencies { get; set; } = new() { "USD", "EUR", "GBP", "CAD" };
    public string DefaultCurrency { get; set; } = "USD";
    public bool SimulateLatency { get; set; } = false;
    public int SimulatedLatencyMs { get; set; } = 1000;
    public double SimulatedFailureRate { get; set; } = 0.0; // 0.0 to 1.0
}
