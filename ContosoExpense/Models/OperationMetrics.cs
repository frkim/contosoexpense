namespace ContosoExpense.Models;

/// <summary>
/// Represents in-memory operation metrics.
/// </summary>
public class OperationMetrics
{
    public long TotalOperations { get; set; }
    public Dictionary<string, long> OperationCounts { get; set; } = new();
    public Dictionary<string, double> AverageLatencyMs { get; set; } = new();
    public Dictionary<string, long> LatencyTotalsMs { get; set; } = new();
}

/// <summary>
/// Represents a single request timing entry.
/// </summary>
public class RequestTiming
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public long ElapsedMs { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? UserId { get; set; }
}
