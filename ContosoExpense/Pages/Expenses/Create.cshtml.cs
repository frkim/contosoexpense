using ContosoExpense.Models.ViewModels;
using ContosoExpense.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoExpense.Pages.Expenses;

public class CreateModel : PageModel
{
    private readonly IExpenseService _expenseService;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISystemSettingsService _settingsService;
    private readonly IAuthService _authService;

    public CreateModel(
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

    public async Task OnGetAsync()
    {
        await LoadFormDataAsync();
    }

    public async Task<IActionResult> OnPostAsync(string action)
    {
        await LoadFormDataAsync();

        if (!ModelState.IsValid)
            return Page();

        var userId = _authService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "You must be logged in to create an expense";
            return RedirectToPage("/Index");
        }

        var result = await _expenseService.CreateExpenseAsync(Input, userId);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return Page();
        }

        if (action == "submit")
        {
            var submitResult = await _expenseService.SubmitExpenseAsync(result.Data!.Id, userId);
            if (!submitResult.Success)
            {
                TempData["ErrorMessage"] = submitResult.ErrorMessage;
                return RedirectToPage("Index");
            }
            TempData["SuccessMessage"] = "Expense created and submitted for approval";
        }
        else
        {
            TempData["SuccessMessage"] = "Expense saved as draft";
        }

        return RedirectToPage("Index");
    }

    private async Task LoadFormDataAsync()
    {
        Input.AvailableCategories = (await _categoryRepository.GetActiveAsync()).ToList();
        Input.AllowedCurrencies = _settingsService.GetSettings().AllowedCurrencies;
    }
}
