using ContosoExpense.Models;
using ContosoExpense.Models.ViewModels;
using ContosoExpense.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoExpense.Pages.Expenses;

public class IndexModel : PageModel
{
    private readonly IExpenseService _expenseService;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAuthService _authService;

    public IndexModel(
        IExpenseService expenseService,
        ICategoryRepository categoryRepository,
        IAuthService authService)
    {
        _expenseService = expenseService;
        _categoryRepository = categoryRepository;
        _authService = authService;
    }

    [BindProperty(SupportsGet = true)]
    public ExpenseFilterViewModel Filter { get; set; } = new();

    public PagedResult<ExpenseListViewModel> Expenses { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public bool IsManager { get; set; }

    public async Task OnGetAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        IsManager = user?.Role == UserRole.Manager;

        Categories = (await _categoryRepository.GetActiveAsync()).ToList();
        Expenses = await _expenseService.GetExpenseListAsync(Filter, user?.Id, IsManager);
    }

    public async Task<IActionResult> OnPostSubmitAsync(string id)
    {
        var userId = _authService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return RedirectToPage();

        var result = await _expenseService.SubmitExpenseAsync(id, userId);
        if (result.Success)
            TempData["SuccessMessage"] = "Expense submitted for approval";
        else
            TempData["ErrorMessage"] = result.ErrorMessage;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostApproveAsync(string expenseId, string? notes)
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user?.Role != UserRole.Manager)
        {
            TempData["ErrorMessage"] = "Only managers can approve expenses";
            return RedirectToPage();
        }

        var result = await _expenseService.ApproveExpenseAsync(expenseId, user.Id, notes);
        if (result.Success)
            TempData["SuccessMessage"] = "Expense approved";
        else
            TempData["ErrorMessage"] = result.ErrorMessage;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(string expenseId, string reason)
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user?.Role != UserRole.Manager)
        {
            TempData["ErrorMessage"] = "Only managers can reject expenses";
            return RedirectToPage();
        }

        var result = await _expenseService.RejectExpenseAsync(expenseId, user.Id, reason);
        if (result.Success)
            TempData["SuccessMessage"] = "Expense rejected";
        else
            TempData["ErrorMessage"] = result.ErrorMessage;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostPayAsync(string id)
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user?.Role != UserRole.Manager)
        {
            TempData["ErrorMessage"] = "Only managers can mark expenses as paid";
            return RedirectToPage();
        }

        var result = await _expenseService.MarkAsPaidAsync(id, user.Id);
        if (result.Success)
            TempData["SuccessMessage"] = "Expense marked as paid";
        else
            TempData["ErrorMessage"] = result.ErrorMessage;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user == null)
            return RedirectToPage();

        var result = await _expenseService.DeleteExpenseAsync(id, user.Id, user.Role == UserRole.Manager);
        if (result.Success)
            TempData["SuccessMessage"] = "Expense deleted";
        else
            TempData["ErrorMessage"] = result.ErrorMessage;

        return RedirectToPage();
    }

    public string GetQueryString(params string[] excludeParams)
    {
        var queryParams = new List<string>();
        
        if (!excludeParams.Contains("SearchTerm") && !string.IsNullOrEmpty(Filter.SearchTerm))
            queryParams.Add($"SearchTerm={Uri.EscapeDataString(Filter.SearchTerm)}");
        
        if (!excludeParams.Contains("Status") && Filter.Status.HasValue)
            queryParams.Add($"Status={Filter.Status}");
        
        if (!excludeParams.Contains("CategoryId") && !string.IsNullOrEmpty(Filter.CategoryId))
            queryParams.Add($"CategoryId={Uri.EscapeDataString(Filter.CategoryId)}");
        
        if (!excludeParams.Contains("DateFrom") && Filter.DateFrom.HasValue)
            queryParams.Add($"DateFrom={Filter.DateFrom:yyyy-MM-dd}");
        
        if (!excludeParams.Contains("DateTo") && Filter.DateTo.HasValue)
            queryParams.Add($"DateTo={Filter.DateTo:yyyy-MM-dd}");
        
        if (!excludeParams.Contains("SortBy") && !string.IsNullOrEmpty(Filter.SortBy))
            queryParams.Add($"SortBy={Filter.SortBy}");
        
        if (!excludeParams.Contains("SortDescending"))
            queryParams.Add($"SortDescending={Filter.SortDescending}");

        return string.Join("&", queryParams);
    }
}
