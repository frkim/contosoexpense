namespace ContosoExpense.Models.ViewModels;

/// <summary>
/// View model for dashboard data.
/// </summary>
public class DashboardViewModel
{
    public string Title { get; set; } = "Dashboard";
    public bool IsPersonal { get; set; }
    public string? UserId { get; set; }
    public string? UserDisplayName { get; set; }
    
    // KPIs
    public int TotalSubmittedThisMonth { get; set; }
    public int TotalApprovedThisMonth { get; set; }
    public int TotalRejectedThisMonth { get; set; }
    public int TotalPendingApproval { get; set; }
    public decimal TotalAmountThisMonth { get; set; }
    public double AverageApprovalTimeHours { get; set; }
    
    // Chart data
    public List<MonthlyExpenseData> MonthlyData { get; set; } = new();
    public List<CategoryExpenseData> CategoryData { get; set; } = new();
    public List<StatusDistributionData> StatusDistribution { get; set; } = new();
    
    // Filter
    public DashboardFilter CurrentFilter { get; set; } = DashboardFilter.ThisMonth;
    public List<User> AvailableUsers { get; set; } = new();
}

/// <summary>
/// Monthly expense aggregation for charts.
/// </summary>
public class MonthlyExpenseData
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public decimal RejectedAmount { get; set; }
    public decimal PendingAmount { get; set; }
}

/// <summary>
/// Category-wise expense aggregation for charts.
/// </summary>
public class CategoryExpenseData
{
    public string CategoryId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Status distribution for pie charts.
/// </summary>
public class StatusDistributionData
{
    public ExpenseStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Dashboard time filter options.
/// </summary>
public enum DashboardFilter
{
    ThisMonth,
    LastThreeMonths,
    YearToDate,
    AllTime
}
