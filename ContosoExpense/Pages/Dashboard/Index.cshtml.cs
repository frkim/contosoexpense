using ContosoExpense.Models;
using ContosoExpense.Models.ViewModels;
using ContosoExpense.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoExpense.Pages.Dashboard;

public class IndexModel : PageModel
{
    private readonly IDashboardService _dashboardService;
    private readonly IAuthService _authService;

    public IndexModel(IDashboardService dashboardService, IAuthService authService)
    {
        _dashboardService = dashboardService;
        _authService = authService;
    }

    public DashboardViewModel Dashboard { get; set; } = new();
    public bool IsManager { get; set; }
    public string? SelectedUserId { get; set; }

    public async Task OnGetAsync(DashboardFilter filter = DashboardFilter.ThisMonth, string? userId = null)
    {
        var user = await _authService.GetCurrentUserAsync();
        IsManager = user?.Role == UserRole.Manager;
        SelectedUserId = userId;

        // Non-managers can only see their own dashboard
        if (!IsManager)
        {
            userId = user?.Id;
            SelectedUserId = userId;
        }

        Dashboard = await _dashboardService.GetDashboardDataAsync(filter, userId);
    }
}
