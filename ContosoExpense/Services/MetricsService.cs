using ContosoExpense.Models;
using System.Collections.Concurrent;

namespace ContosoExpense.Services;

/// <summary>
/// Interface for metrics service.
/// </summary>
public interface IMetricsService
{
    void RecordOperation(string operationType, long elapsedMs);
    void RecordRequest(string path, string method, long elapsedMs, string? userId);
    OperationMetrics GetMetrics();
    IEnumerable<RequestTiming> GetRecentRequests(int count = 100);
    void Reset();
}

/// <summary>
/// In-memory metrics service for observability.
/// </summary>
public class InMemoryMetricsService : IMetricsService
{
    private readonly ConcurrentDictionary<string, long> _operationCounts = new();
    private readonly ConcurrentDictionary<string, long> _latencyTotals = new();
    private readonly ConcurrentQueue<RequestTiming> _recentRequests = new();
    private long _totalOperations;
    private readonly object _lock = new();

    public void RecordOperation(string operationType, long elapsedMs)
    {
        _operationCounts.AddOrUpdate(operationType, 1, (_, count) => count + 1);
        _latencyTotals.AddOrUpdate(operationType, elapsedMs, (_, total) => total + elapsedMs);
        Interlocked.Increment(ref _totalOperations);
    }

    public void RecordRequest(string path, string method, long elapsedMs, string? userId)
    {
        var timing = new RequestTiming
        {
            Path = path,
            Method = method,
            ElapsedMs = elapsedMs,
            UserId = userId,
            Timestamp = DateTime.UtcNow
        };

        _recentRequests.Enqueue(timing);

        // Keep only last 1000 requests
        while (_recentRequests.Count > 1000)
        {
            _recentRequests.TryDequeue(out _);
        }
    }

    public OperationMetrics GetMetrics()
    {
        var metrics = new OperationMetrics
        {
            TotalOperations = _totalOperations,
            OperationCounts = new Dictionary<string, long>(_operationCounts),
            LatencyTotalsMs = new Dictionary<string, long>(_latencyTotals)
        };

        foreach (var op in metrics.OperationCounts.Keys)
        {
            if (metrics.OperationCounts.TryGetValue(op, out var count) && 
                metrics.LatencyTotalsMs.TryGetValue(op, out var total) && 
                count > 0)
            {
                metrics.AverageLatencyMs[op] = (double)total / count;
            }
        }

        return metrics;
    }

    public IEnumerable<RequestTiming> GetRecentRequests(int count = 100)
    {
        return _recentRequests.TakeLast(count).Reverse();
    }

    public void Reset()
    {
        _operationCounts.Clear();
        _latencyTotals.Clear();
        while (_recentRequests.TryDequeue(out _)) { }
        Interlocked.Exchange(ref _totalOperations, 0);
    }
}
