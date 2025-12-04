using ContosoExpense.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoExpense.Pages;

public class SwitchUserModel : PageModel
{
    private readonly IAuthService _authService;

    public SwitchUserModel(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<IActionResult> OnPostAsync(string userId)
    {
        await _authService.SwitchUserAsync(HttpContext, userId);
        TempData["SuccessMessage"] = "User switched successfully";
        return RedirectToPage("/Index");
    }
}
