using ContosoExpense.Models;
using ContosoExpense.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoExpense.Pages;

public class IndexModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly IExpenseRepository _expenseRepository;

    public bool IsManager { get; set; }
    public int TotalExpenses { get; set; }
    public int PendingApproval { get; set; }
    public decimal TotalAmount { get; set; }

    public IndexModel(IAuthService authService, IExpenseRepository expenseRepository)
    {
        _authService = authService;
        _expenseRepository = expenseRepository;
    }

    public async Task OnGetAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        IsManager = user?.Role == UserRole.Manager;

        var expenses = await _expenseRepository.GetAllAsync();
        var expenseList = expenses.ToList();

        // For non-managers, show only their expenses
        if (!IsManager && user != null)
        {
            expenseList = expenseList.Where(e => e.UserId == user.Id).ToList();
        }

        TotalExpenses = expenseList.Count;
        PendingApproval = expenseList.Count(e => e.Status == ExpenseStatus.Submitted);

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        TotalAmount = expenseList
            .Where(e => e.ExpenseDate >= startOfMonth && e.Status != ExpenseStatus.Rejected)
            .Sum(e => e.Amount);
    }
}
