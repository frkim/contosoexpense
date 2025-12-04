using ContosoExpense.Models;
using ContosoExpense.Models.ViewModels;
using System.Globalization;

namespace ContosoExpense.Services;

/// <summary>
/// Interface for dashboard data service.
/// </summary>
public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync(DashboardFilter filter, string? userId = null);
}

/// <summary>
/// Dashboard data aggregation service.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserRepository _userRepository;

    public DashboardService(
        IExpenseRepository expenseRepository,
        ICategoryRepository categoryRepository,
        IUserRepository userRepository)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
    }

    public async Task<DashboardViewModel> GetDashboardDataAsync(DashboardFilter filter, string? userId = null)
    {
        var (startDate, endDate) = GetDateRangeForFilter(filter);
        var expenses = await _expenseRepository.GetExpensesForDateRangeAsync(startDate, endDate, userId);
        var categories = (await _categoryRepository.GetAllAsync()).ToDictionary(c => c.Id);
        var users = (await _userRepository.GetAllAsync()).ToList();

        var expenseList = expenses.ToList();
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        // Filter for current month KPIs
        var thisMonthExpenses = expenseList.Where(e => 
            e.ExpenseDate >= startOfMonth && e.ExpenseDate <= endOfMonth).ToList();

        var dashboard = new DashboardViewModel
        {
            IsPersonal = !string.IsNullOrEmpty(userId),
            UserId = userId,
            CurrentFilter = filter,
            AvailableUsers = users,

            // KPIs for current month
            TotalSubmittedThisMonth = thisMonthExpenses.Count(e => e.Status == ExpenseStatus.Submitted),
            TotalApprovedThisMonth = thisMonthExpenses.Count(e => e.Status == ExpenseStatus.Approved || e.Status == ExpenseStatus.Paid),
            TotalRejectedThisMonth = thisMonthExpenses.Count(e => e.Status == ExpenseStatus.Rejected),
            TotalPendingApproval = expenseList.Count(e => e.Status == ExpenseStatus.Submitted),
            TotalAmountThisMonth = thisMonthExpenses.Where(e => e.Status != ExpenseStatus.Rejected).Sum(e => e.Amount),
            AverageApprovalTimeHours = CalculateAverageApprovalTime(expenseList),

            // Chart data
            MonthlyData = GetMonthlyData(expenseList, startDate, endDate),
            CategoryData = GetCategoryData(expenseList, categories),
            StatusDistribution = GetStatusDistribution(expenseList)
        };

        if (!string.IsNullOrEmpty(userId))
        {
            var user = users.FirstOrDefault(u => u.Id == userId);
            dashboard.UserDisplayName = user?.DisplayName;
            dashboard.Title = $"{user?.DisplayName}'s Dashboard";
        }
        else
        {
            dashboard.Title = "Company Dashboard";
        }

        return dashboard;
    }

    private static (DateTime startDate, DateTime endDate) GetDateRangeForFilter(DashboardFilter filter)
    {
        var now = DateTime.UtcNow;
        var endDate = now;

        return filter switch
        {
            DashboardFilter.ThisMonth => (new DateTime(now.Year, now.Month, 1), endDate),
            DashboardFilter.LastThreeMonths => (now.AddMonths(-3), endDate),
            DashboardFilter.YearToDate => (new DateTime(now.Year, 1, 1), endDate),
            DashboardFilter.AllTime => (DateTime.MinValue, endDate),
            _ => (new DateTime(now.Year, now.Month, 1), endDate)
        };
    }

    private static double CalculateAverageApprovalTime(List<Expense> expenses)
    {
        var approvedExpenses = expenses
            .Where(e => e.ApprovedAt.HasValue && e.SubmittedAt.HasValue)
            .ToList();

        if (!approvedExpenses.Any())
            return 0;

        var totalHours = approvedExpenses
            .Sum(e => (e.ApprovedAt!.Value - e.SubmittedAt!.Value).TotalHours);

        return totalHours / approvedExpenses.Count;
    }

    private static List<MonthlyExpenseData> GetMonthlyData(List<Expense> expenses, DateTime startDate, DateTime endDate)
    {
        var result = new List<MonthlyExpenseData>();
        var current = new DateTime(startDate.Year, startDate.Month, 1);
        var end = new DateTime(endDate.Year, endDate.Month, 1);

        while (current <= end)
        {
            var monthExpenses = expenses.Where(e =>
                e.ExpenseDate.Year == current.Year &&
                e.ExpenseDate.Month == current.Month).ToList();

            result.Add(new MonthlyExpenseData
            {
                Year = current.Year,
                Month = current.Month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(current.Month),
                Count = monthExpenses.Count,
                TotalAmount = monthExpenses.Sum(e => e.Amount),
                ApprovedAmount = monthExpenses.Where(e => e.Status == ExpenseStatus.Approved || e.Status == ExpenseStatus.Paid).Sum(e => e.Amount),
                RejectedAmount = monthExpenses.Where(e => e.Status == ExpenseStatus.Rejected).Sum(e => e.Amount),
                PendingAmount = monthExpenses.Where(e => e.Status == ExpenseStatus.Submitted || e.Status == ExpenseStatus.Draft).Sum(e => e.Amount)
            });

            current = current.AddMonths(1);
        }

        return result;
    }

    private static List<CategoryExpenseData> GetCategoryData(List<Expense> expenses, Dictionary<string, Category> categories)
    {
        var totalAmount = expenses.Sum(e => e.Amount);

        return expenses
            .GroupBy(e => e.CategoryId)
            .Select(g =>
            {
                var category = categories.GetValueOrDefault(g.Key);
                var amount = g.Sum(e => e.Amount);
                return new CategoryExpenseData
                {
                    CategoryId = g.Key,
                    CategoryName = category?.Name ?? "Unknown",
                    CategoryIcon = category?.Icon ?? "bi-tag",
                    Count = g.Count(),
                    TotalAmount = amount,
                    Percentage = totalAmount > 0 ? Math.Round((amount / totalAmount) * 100, 1) : 0
                };
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();
    }

    private static List<StatusDistributionData> GetStatusDistribution(List<Expense> expenses)
    {
        var total = expenses.Count;
        if (total == 0) total = 1; // Avoid division by zero

        return Enum.GetValues<ExpenseStatus>()
            .Select(status =>
            {
                var count = expenses.Count(e => e.Status == status);
                return new StatusDistributionData
                {
                    Status = status,
                    StatusName = status.ToString(),
                    Count = count,
                    Percentage = Math.Round((count / (decimal)total) * 100, 1)
                };
            })
            .Where(s => s.Count > 0)
            .OrderByDescending(s => s.Count)
            .ToList();
    }
}
