using ContosoExpense.Models;
using ContosoExpense.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace ContosoExpense.Pages.Admin;

public class SettingsModel : PageModel
{
    private readonly ISystemSettingsService _settingsService;
    private readonly IDataManagementService _dataManagementService;
    private readonly IAuthService _authService;

    public SettingsModel(
        ISystemSettingsService settingsService,
        IDataManagementService dataManagementService,
        IAuthService authService)
    {
        _settingsService = settingsService;
        _dataManagementService = dataManagementService;
        _authService = authService;
    }

    public SystemSettings Settings { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user?.Role != UserRole.Manager)
        {
            TempData["ErrorMessage"] = "Access denied. Managers only.";
            return RedirectToPage("/Index");
        }

        Settings = _settingsService.GetSettings();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string defaultCurrency, decimal receiptThreshold, 
        bool simulateLatency, int latencyMs, int failureRate)
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user?.Role != UserRole.Manager)
            return RedirectToPage("/Index");

        var settings = _settingsService.GetSettings();
        settings.DefaultCurrency = defaultCurrency;
        settings.ReceiptRequiredThreshold = receiptThreshold;
        settings.SimulateLatency = simulateLatency;
        settings.SimulatedLatencyMs = latencyMs;
        settings.SimulatedFailureRate = failureRate / 100.0;

        _settingsService.UpdateSettings(settings);
        TempData["SuccessMessage"] = "Settings saved";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostExportAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user?.Role != UserRole.Manager)
            return RedirectToPage("/Index");

        var json = await _dataManagementService.ExportDataAsync();
        var bytes = Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", $"contoso-expense-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");
    }

    public async Task<IActionResult> OnPostResetAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user?.Role != UserRole.Manager)
            return RedirectToPage("/Index");

        await _dataManagementService.ResetAllDataAsync();
        TempData["SuccessMessage"] = "All data has been reset to initial sample state";
        return RedirectToPage();
    }
}
