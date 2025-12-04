using ContosoExpense.Models.ViewModels;
using ContosoExpense.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoExpense.Pages.Expenses;

public class EditModel : PageModel
{
    private readonly IExpenseService _expenseService;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISystemSettingsService _settingsService;
    private readonly IAuthService _authService;

    public EditModel(
        IExpenseService expenseService,
        ICategoryRepository categoryRepository,
        ISystemSettingsService settingsService,
        IAuthService authService)
    {
        _expenseService = expenseService;
        _categoryRepository = categoryRepository;
        _settingsService = settingsService;
        _authService = authService;
    }

    [BindProperty]
    public ExpenseFormViewModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var expense = await _expenseService.GetExpenseAsync(id);
        if (expense == null)
        {
            TempData["ErrorMessage"] = "Expense not found";
            return RedirectToPage("Index");
        }

        var userId = _authService.GetCurrentUserId();
        if (expense.UserId != userId)
        {
            TempData["ErrorMessage"] = "You can only edit your own expenses";
            return RedirectToPage("Index");
        }

        if (expense.Status != Models.ExpenseStatus.Draft && expense.Status != Models.ExpenseStatus.Rejected)
        {
            TempData["ErrorMessage"] = "Only draft or rejected expenses can be edited";
            return RedirectToPage("Details", new { id });
        }

        await LoadFormDataAsync();

        Input.Id = expense.Id;
        Input.Title = expense.Title;
        Input.Description = expense.Description;
        Input.Amount = expense.Amount;
        Input.Currency = expense.Currency;
        Input.CategoryId = expense.CategoryId;
        Input.ExpenseDate = expense.ExpenseDate;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string action)
    {
        await LoadFormDataAsync();

        if (!ModelState.IsValid)
            return Page();

        var userId = _authService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "You must be logged in";
            return RedirectToPage("/Index");
        }

        var result = await _expenseService.UpdateExpenseAsync(Input.Id!, Input, userId);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return Page();
        }

        if (action == "submit")
        {
            var submitResult = await _expenseService.SubmitExpenseAsync(Input.Id!, userId);
            if (!submitResult.Success)
            {
                TempData["ErrorMessage"] = submitResult.ErrorMessage;
                return RedirectToPage("Details", new { id = Input.Id });
            }
            TempData["SuccessMessage"] = "Expense updated and submitted for approval";
        }
        else
        {
            TempData["SuccessMessage"] = "Expense updated";
        }

        return RedirectToPage("Details", new { id = Input.Id });
    }

    private async Task LoadFormDataAsync()
    {
        Input.AvailableCategories = (await _categoryRepository.GetActiveAsync()).ToList();
        Input.AllowedCurrencies = _settingsService.GetSettings().AllowedCurrencies;
    }
}
