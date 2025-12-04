using ContosoExpense.Models;
using ContosoExpense.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoExpense.Pages.Admin;

public class MetricsModel : PageModel
{
    private readonly IMetricsService _metricsService;
    private readonly IAuthService _authService;

    public MetricsModel(IMetricsService metricsService, IAuthService authService)
    {
        _metricsService = metricsService;
        _authService = authService;
    }

    public OperationMetrics Metrics { get; set; } = new();
    public IEnumerable<RequestTiming> RecentRequests { get; set; } = new List<RequestTiming>();
    public double AverageLatency { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user?.Role != UserRole.Manager)
        {
            TempData["ErrorMessage"] = "Access denied. Managers only.";
            return RedirectToPage("/Index");
        }

        Metrics = _metricsService.GetMetrics();
        RecentRequests = _metricsService.GetRecentRequests(100);
        AverageLatency = Metrics.AverageLatencyMs.Values.Any() 
            ? Metrics.AverageLatencyMs.Values.Average() 
            : 0;

        return Page();
    }

    public async Task<IActionResult> OnPostClearAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user?.Role != UserRole.Manager)
            return RedirectToPage("/Index");

        _metricsService.Reset();

        TempData["SuccessMessage"] = "Metrics cleared";
        return RedirectToPage();
    }
}
