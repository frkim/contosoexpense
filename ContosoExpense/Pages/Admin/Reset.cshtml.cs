using ContosoExpense.Models;
using ContosoExpense.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoExpense.Pages.Admin;

public class ResetModel : PageModel
{
    private readonly IDataManagementService _dataManagementService;
    private readonly IAuthService _authService;

    public ResetModel(IDataManagementService dataManagementService, IAuthService authService)
    {
        _dataManagementService = dataManagementService;
        _authService = authService;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user?.Role != UserRole.Manager)
        {
            TempData["ErrorMessage"] = "Access denied. Managers only.";
            return RedirectToPage("/Index");
        }

        await _dataManagementService.ResetAllDataAsync();
        TempData["SuccessMessage"] = "All data has been reset to initial sample state";
        return RedirectToPage("/Index");
    }
}
