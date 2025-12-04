using ContosoExpense.Models;
using ContosoExpense.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoExpense.Pages.Admin;

public class UsersModel : PageModel
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public UsersModel(IUserRepository userRepository, IAuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    public List<User> Users { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user?.Role != UserRole.Manager)
        {
            TempData["ErrorMessage"] = "Access denied. Managers only.";
            return RedirectToPage("/Index");
        }

        Users = (await _userRepository.GetAllAsync()).ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(string username, string displayName, string email, string? department, string role)
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser?.Role != UserRole.Manager)
            return RedirectToPage("/Index");

        await _userRepository.CreateAsync(new User
        {
            Username = username,
            DisplayName = displayName,
            Email = email,
            Department = department ?? string.Empty,
            Role = Enum.Parse<UserRole>(role),
            IsActive = true
        });

        TempData["SuccessMessage"] = "User added";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostPromoteToManagerAsync(string id)
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser?.Role != UserRole.Manager)
            return RedirectToPage("/Index");

        var user = await _userRepository.GetByIdAsync(id);
        if (user != null)
        {
            user.Role = UserRole.Manager;
            await _userRepository.UpdateAsync(user);
            TempData["SuccessMessage"] = $"{user.DisplayName} promoted to Manager";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDemoteToUserAsync(string id)
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser?.Role != UserRole.Manager)
            return RedirectToPage("/Index");

        var user = await _userRepository.GetByIdAsync(id);
        if (user != null)
        {
            user.Role = UserRole.User;
            await _userRepository.UpdateAsync(user);
            TempData["SuccessMessage"] = $"{user.DisplayName} demoted to User";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(string id)
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser?.Role != UserRole.Manager)
            return RedirectToPage("/Index");

        var user = await _userRepository.GetByIdAsync(id);
        if (user != null)
        {
            user.IsActive = false;
            await _userRepository.UpdateAsync(user);
            TempData["SuccessMessage"] = "User deactivated";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostActivateAsync(string id)
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser?.Role != UserRole.Manager)
            return RedirectToPage("/Index");

        var user = await _userRepository.GetByIdAsync(id);
        if (user != null)
        {
            user.IsActive = true;
            await _userRepository.UpdateAsync(user);
            TempData["SuccessMessage"] = "User activated";
        }

        return RedirectToPage();
    }
}
